using AutoMapper;
using Common.Exceptions;
using Dal.Data;
using Dal.Models;
using Dto.Comment;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Comment;

public class CommentService: ICommentService
{
    private readonly ChatDbContext _db;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentDto> _createValidator;
    public CommentService(ChatDbContext db, IMapper mapper, IValidator<CreateCommentDto> createValidator)
    {
        _db = db;
        _mapper = mapper;
        _createValidator = createValidator;
    }
    #region create
    public async Task<int> CreateCommentAsync(CreateCommentDto dto)
    {
        _createValidator.ValidateAndThrow(dto);
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var currentUserId = await GetCurrentUserId(dto);
            var commentEntity = _mapper.Map<CommentModel>(dto);
            commentEntity.UserId = currentUserId;
            commentEntity.CreatedAt = DateTime.UtcNow;
            _db.Comments.Add(commentEntity);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return commentEntity.Id;
        }
        catch (Exception ex)
        { 
            await transaction.RollbackAsync();
            throw new BaseException(ex.Message, "CreateCommentAsync", "Comment Service error", HttpStatusCode.BadRequest);
        }
    }

    private async Task<int> GetCurrentUserId(CreateCommentDto dto)
    {
        var entityId = await _db.Users.Where(a=>a.UserName.ToUpper() == dto.UserName.Trim().ToUpper())
            .Select(a=>a.Id)
            .FirstOrDefaultAsync();
        if(entityId > 0)
        {
            return entityId;
        }
        else {
            var newUser = _mapper.Map<UserModel>(dto);
            newUser.CreatedAt = DateTime.UtcNow;
            _db.Users.Add(newUser);
            await  _db.SaveChangesAsync();
            return newUser.Id;
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

            IOrderedQueryable<CommentModel> queryOrdered;
            // Начальный запрос для комментариев, где ParentId == null
            var query = _db.Comments.Where(c => c.ParentCommentId == null)
                                     .Include(a => a.User) // Загрузим User для каждого комментария
                                     .Include(c => c.Replies) // Загрузим все ответы на комментарий
                                     .ThenInclude(r => r.User); // Загрузим User для каждого ответа

            // Если поле для сортировки указано, создаем динамическое выражение сортировки
            if (!string.IsNullOrEmpty(sortField))
            {
                // Определяем параметр для сортировки
                var param = Expression.Parameter(typeof(CommentModel), "c");
                Expression property;

                // Проверяем, какое поле для сортировки
                if (sortField.Equals("userName", StringComparison.OrdinalIgnoreCase))
                {
                    // Сортировка по полю UserName из связанной модели User
                    property = Expression.Property(Expression.Property(param, "User"), "UserName");
                }
                else if (sortField.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    // Сортировка по полю Email из связанной модели User
                    property = Expression.Property(Expression.Property(param, "User"), "Email");
                }
                else if (sortField.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                {
                    // Сортировка по полю CreatedAt из основной модели CommentModel
                    property = Expression.Property(param, "CreatedAt");
                }
                else
                {
                    // В случае неизвестного поля, сортировка по умолчанию по CreatedAt
                    property = Expression.Property(param, "CreatedAt");
                }

                var lambda = Expression.Lambda<Func<CommentModel, object>>(Expression.Convert(property, typeof(object)), param);

                // Проверяем, какое направление сортировки передано
                if (string.IsNullOrEmpty(sort) || sort.ToLower() == "desc")
                {
                    // Сортировка по убыванию
                    queryOrdered = query.OrderByDescending(lambda);
                }
                else
                {
                    // Сортировка по возрастанию
                    queryOrdered = query.OrderBy(lambda);
                }
            }
            else
            {
                // Если не указано поле сортировки, по умолчанию сортируем по дате в порядке убывания
                queryOrdered = query.OrderByDescending(c => c.CreatedAt);
            }

            // Получаем общее количество записей
            response.TotalCount = await queryOrdered.CountAsync();

            // Получаем данные для текущей страницы
            var items = await queryOrdered.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            if (items != null)
            {
                var dataMap = _mapper.Map<List<CommentDto>>(items);
                response.Data = dataMap;
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new BaseException(ex.Message, "GetParentCommentList", "Comment Service error", HttpStatusCode.BadRequest);
        }
    }



    #endregion view
}
