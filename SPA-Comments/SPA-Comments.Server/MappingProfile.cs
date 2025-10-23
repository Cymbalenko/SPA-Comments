using AutoMapper;
using Dal.Models;
using Dto.Comment;

namespace SPA_Comments.Server;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        #region Comment
        CreateMap<CommentDto, CommentModel>()
        .ForMember(v => v.ParentId, opt => opt.MapFrom(t => t.ParentCommentId));
        CreateMap<CommentModel, CommentDto>()
        .ForMember(v => v.ParentCommentId, opt => opt.MapFrom(t => t.ParentId))
        .AfterMap((entity, model) =>
        {
            if(entity.User != null)
            {
                model.UserName = entity.User.UserName;
                model.HomePage = entity.User.HomePage;
                model.Email = entity.User.Email;
            }
        });
        CreateMap<CreateCommentDto, CommentModel>()
            .ForMember(v => v.ParentId, opt => opt.MapFrom(t => t.ParentCommentId ));
        CreateMap<CommentModel, CreateCommentDto>()
            .ForMember(v => v.ParentCommentId, opt => opt.MapFrom(t => t.ParentId));

        CreateMap<CreateCommentDto, UserModel>();
        CreateMap<UserModel, CreateCommentDto>();

        CreateMap<UserModel, CommentDto>();
        CreateMap<CommentDto, UserModel>();
        #endregion Comment
    }
}

