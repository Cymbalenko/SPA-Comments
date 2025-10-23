using AutoMapper;
using Dal.Models;
using Dto.Comment;

namespace SPA_Comments.Server;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        #region Comment
        CreateMap<CommentDto, CommentModel>();
        CreateMap<CommentModel, CommentDto>()
        .AfterMap((entity, model) =>
        {
            if(entity.User != null)
            {
                model.UserName = entity.User.UserName;
                model.HomePage = entity.User.HomePage;
                model.Email = entity.User.Email;
            }
        }); ;
        CreateMap<CreateCommentDto, CommentModel>();
        CreateMap<CommentModel, CreateCommentDto>();
        CreateMap<CreateCommentDto, UserModel>();
        CreateMap<UserModel, CreateCommentDto>();
        CreateMap<UserModel, CommentDto>();
        CreateMap<CommentDto, UserModel>();
        #endregion Comment
    }
}

