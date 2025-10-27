using AutoMapper;
using Common.Exceptions;
using Common.Helper;
using Dal.Data;
using Dal.Models;
using Dto.Comment;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using Service.Messaging;
using Service.Services.Azure;
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
    private readonly IFileStorageService _fileServise;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ChatDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentDto> _createValidator;
    private readonly ILogger<CommentService> _logger;
    public CommentService(ChatDbContext db, IMapper mapper, IValidator<CreateCommentDto> createValidator, IRabbitMqPublisher rabbitMqPublisher, IElasticClient elasticClient, ILogger<CommentService> logger, IFileStorageService fileServise)
    {
        _db = db;
        _mapper = mapper;
        _createValidator = createValidator;
        _rabbitMqPublisher = rabbitMqPublisher;
        _elasticClient = elasticClient;
        _logger = logger;
        _fileServise = fileServise;
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
            commentEntity.Files = new List<FileModel>();
            //addFiles
            if (dto.Files != null && dto.Files.Count > 0)
            { 
                foreach (var item in dto.Files)
                {
                    var info = FileHelper.GetFileInfo(item); 
                    var url = await _fileServise.UploadFileAsync(item, "test-user");
                    commentEntity.Files.Add(new()
                    {
                        UploadedAt = DateTime.Now,
                        Uri = url,
                        Name = info.FileName,
                        ContentType = info.ContentType,
                        IsImage = info.IsImage
                    });
                } 
            }

            _db.Comments.Add(commentEntity);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Отправляем новый комментарий в очередь RabbitMQ
            var comentDto = _mapper.Map<CommentDto>(commentEntity);
            if(commentEntity.Files.Count > 0)
            {
                foreach (var item in commentEntity.Files)
                {
                    var fileMap = _mapper.Map<CommentFileDto>(item);
                    comentDto.Files.Add(fileMap);
                }
            }
            await _rabbitMqPublisher.PublishAsync(RabbitMqPublisher.RabbitCommentQueue, "new_comment", comentDto);

            return commentEntity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,ex.Message);
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
            _logger.LogError(ex, ex.Message);
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

    public async Task<List<CommentDto>> GetCommentsTreeAsync(int parentId)
    {
        // 1. Получаем все комментарии
        var searchResponse = await _elasticClient.SearchAsync<CommentDto>(s => s
            .Index("comments")
            .Size(10000)
            .Query(q => q.MatchAll())
        );

        if (!searchResponse.IsValid)
            return new List<CommentDto>();

        var allComments = searchResponse.Documents.ToList();

        var childCounts = allComments
        .Where(c => c.ParentId.HasValue)
        .GroupBy(c => c.ParentId.Value)
        .ToDictionary(g => g.Key, g => g.Count());

        // Добавляем RepliesCount
        foreach (var comment in allComments)
        {
            comment.RepliesCount = childCounts.ContainsKey(comment.Id)
                ? childCounts[comment.Id]
                : 0;
        }

        // 2. Строим дерево комментариев
        var commentDict = allComments.ToDictionary(c => c.Id);

        foreach (var comment in allComments)
        {
            if (comment.ParentId.HasValue && commentDict.ContainsKey(comment.ParentId.Value))
            {
                commentDict[comment.ParentId.Value].Replies ??= new List<CommentDto>();
                commentDict[comment.ParentId.Value].Replies.Add(comment);
            }
        }

        // 3. Возвращаем все комментарии, у которых ParentId = parentId
        return allComments.Where(c => c.ParentId == parentId).ToList();
    }


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
     

    #endregion view
}
