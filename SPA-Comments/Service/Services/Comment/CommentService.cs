using AutoMapper;
using Common.Exceptions;
using Dal.Data;
using Dal.Models;
using Dto.Comment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Comment;

public class CommentService: ICommentService
{
    private readonly ChatDbContext _db;
    private readonly IMapper _mapper;
    public CommentService(ChatDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    #region create
    public async Task<int> CreateCommentAsync(CreateCommentDto dto)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var currentUserId = await GetCurrentUserId(dto);
            var commentEntity = _mapper.Map<CommentModel>(dto);
            commentEntity.UserId = currentUserId;
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
    public async Task<GetCommentListResponse> GetParentCommentListAsync(int page = 1, int pageSize = 25, string sort = "desc")
    {
        try
        {
            var response = new GetCommentListResponse
            {
                Page = page,
                PageSize = pageSize
            };
            var query = _db.Comments.Where(c => c.ParentId == null)
                                    .OrderByDescending(c => c.CreatedAt);

            response.TotalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                                .Include(c => c.Replies)
                                .ToListAsync();

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
