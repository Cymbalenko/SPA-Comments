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
        .ForMember(v => v.Files, opt => opt.Ignore());

        CreateMap<CommentModel, CommentDto>()
        .ForMember(v => v.Files, opt => opt.Ignore())
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
            .ForMember(v => v.Files, opt => opt.Ignore());

        CreateMap<CommentModel, CreateCommentDto>()
            .ForMember(v => v.Files, opt => opt.Ignore());

        CreateMap<CreateCommentDto, UserModel>();
        CreateMap<UserModel, CreateCommentDto>();

        CreateMap<UserModel, CommentDto>();
        CreateMap<CommentDto, UserModel>();
        #endregion Comment

        #region File
        CreateMap<CommentFileDto, FileModel>();
        CreateMap<FileModel, CommentFileDto>();
        #endregion File
    }
}

