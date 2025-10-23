using AutoMapper;
using Common.Exceptions;
using Dal.Data;
using Dal.Models;
using Dto.Comment;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nest;
using Service.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SortOrder = Nest.SortOrder;

namespace Service.Services.Comment;

public class CommentService: ICommentService
{
    private readonly IElasticClient _elasticClient;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ChatDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentDto> _createValidator;
    public CommentService(ChatDbContext db, IMapper mapper, IValidator<CreateCommentDto> createValidator, IRabbitMqPublisher rabbitMqPublisher, IElasticClient elasticClient)
    {
        _db = db;
        _mapper = mapper;
        _createValidator = createValidator;
        _rabbitMqPublisher = rabbitMqPublisher;
        _elasticClient = elasticClient;
    }
    #region create
    public async Task<int> CreateCommentAsync(CreateCommentDto dto)
    {
        _createValidator.ValidateAndThrow(dto);
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var currentUser = await GetCurrentUser(dto);
            var commentEntity = _mapper.Map<CommentModel>(dto);
            commentEntity.UserId = currentUser.Id;
            commentEntity.User = currentUser;
            commentEntity.CreatedAt = DateTime.UtcNow;
            _db.Comments.Add(commentEntity);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Отправляем новый комментарий в очередь RabbitMQ
            var comentDto = _mapper.Map<CommentDto>(commentEntity);
            await _rabbitMqPublisher.PublishAsync(RabbitMqPublisher.RabbitCommentExcenge, "new_comment", comentDto);

            return commentEntity.Id;
        }
        catch (Exception ex)
        { 
            await transaction.RollbackAsync();
            throw new BaseException(ex.Message, "CreateCommentAsync", "Comment Service error", HttpStatusCode.BadRequest);
        }
    }

    private async Task<UserModel> GetCurrentUser(CreateCommentDto dto)
    {
        var entity = await _db.Users.Where(a=>a.UserName.ToUpper() == dto.UserName.Trim().ToUpper())
            .FirstOrDefaultAsync();
        if(entity != null)
        {
            return entity;
        }
        else {
            var newUser = _mapper.Map<UserModel>(dto);
            newUser.CreatedAt = DateTime.UtcNow;
            _db.Users.Add(newUser);
            await  _db.SaveChangesAsync();
            return newUser;
        }
    }
    private string GetFileContentType(string fileExtension)
    {
        string contentType = "application/octet-stream"; // По умолчанию

        switch (fileExtension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                contentType = "image/jpeg";
                break;
            case ".png":
                contentType = "image/png";
                break;
            case ".gif":
                contentType = "image/gif";
                break;
            case ".tif":
            case ".tiff":
                contentType = "image/tiff";
                break;
            case ".svg":
            case ".svgz":
                contentType = "image/svg+xml";
                break;
            case ".pdf":
                contentType = "application/pdf";
                break;
            case ".html":
                contentType = "text/html";
                break;
            case ".txt":
                contentType = "text/plain";
                break;
        }

        return contentType;
    }
    #endregion create

    #region view 
    public async Task<GetCommentListResponse> GetParentCommentListAsync(int page, int pageSize, string sort, string sortField)
    {
        try
        {
            var response = new GetCommentListResponse
            {
                Page = page,
                PageSize = pageSize
            };
             
            var searchResponse = await SearchInElasticAsync(page, pageSize, sort, sortField);

            if (searchResponse != null && searchResponse.IsValid)
            {
                // Если данные найдены в Elasticsearch
                response.TotalCount = (int)searchResponse.Total;
                response.Data = _mapper.Map<List<CommentDto>>(searchResponse.Documents.ToList());
            } 

            return response;
        }
        catch (Exception ex)
        {
            throw new BaseException(ex.Message, "GetParentCommentList", "Comment Service error", HttpStatusCode.BadRequest);
        }
    }

    // 2. Функция для поиска в Elasticsearch
    private async Task<ISearchResponse<CommentDto>> SearchInElasticAsync(
     int page,
     int pageSize,
     string sort,
     string sortField,
     bool onlyRootComments = true
 )
    {
        var from = (page - 1) * pageSize;
        var order = sort.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? SortOrder.Descending
            : SortOrder.Ascending;

        // ✅ Создаём BoolQuery сразу с нужными условиями
        var boolQuery = new BoolQuery
        {
            MustNot = onlyRootComments
                ? new List<QueryContainer>
                {
                new ExistsQuery { Field = "parentId" }
                }
                : null
        };

        // ✅ Формируем поисковый запрос
        var searchRequest = new SearchRequest<CommentDto>("comments")
        {
            From = from,
            Size = pageSize,
            Sort = new List<ISort>
        {
            new FieldSort
            {
                Field = GetSortField(sortField),
                Order = order
            }
        },
            Query = boolQuery
        };

        var searchResponse = await _elasticClient.SearchAsync<CommentDto>(searchRequest);

        if (!searchResponse.IsValid)
        {
            Console.WriteLine($"Elastic error: {searchResponse.OriginalException?.Message}");
            return new SearchResponse<CommentDto>();
        }

        return searchResponse;
    }





    // 3. Функция для поиска в базе данных, если нет данных в Elasticsearch
    private async Task<List<CommentModel>> SearchInDatabaseAsync(int page, int pageSize, string sort, string sortField)
    {
        var query = _db.Comments.Where(c => c.ParentId == null)
                                 .Include(a => a.User)
                                 .Include(c => c.Replies)
                                 .ThenInclude(r => r.User);

       var res = ApplySorting(query, sort, sortField);

        return await res.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    // 4. Функция для применения сортировки к запросу
    private IQueryable<CommentModel> ApplySorting(IQueryable<CommentModel> query, string sort, string sortField)
    {
        if (!string.IsNullOrEmpty(sortField))
        {
            var param = Expression.Parameter(typeof(CommentModel), "c");
            Expression property = Expression.Property(param, sortField);

            var lambda = Expression.Lambda<Func<CommentModel, object>>(Expression.Convert(property, typeof(object)), param);

            return string.IsNullOrEmpty(sort) || sort.ToLower() == "desc"
                ? query.OrderByDescending(lambda)
                : query.OrderBy(lambda);
        }

        return query.OrderByDescending(c => c.CreatedAt);  // По умолчанию сортировка по CreatedAt
    }

    // 5. Функция для получения поля сортировки
    private string GetSortField(string sortField)
    {
        return sortField switch
        {
            "userName" => "userName.keyword",
            "email" => "email.keyword",
            "createdAt" => "createdAt",
            _ => "createdAt"
        };
    }

    // 6. Функция для добавления данных в Elasticsearch
    private async Task AddToElasticAsync(List<CommentModel> items)
    {
        var bulkIndexResponse = await _elasticClient.BulkAsync(b => b
            .IndexMany(items)  // Добавляем все найденные данные в Elasticsearch
        );

        if (!bulkIndexResponse.IsValid)
        {
            // Обработка ошибок индексации
            throw new BaseException("Error indexing to Elasticsearch", "GetParentCommentList", "Elastic indexing error", HttpStatusCode.InternalServerError);
        }
    }

    #endregion view
}
