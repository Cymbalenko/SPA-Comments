using Dto.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.GraphQL.Comment;

public class CommentDtoType : ObjectType<CommentDto>
{
    protected override void Configure(IObjectTypeDescriptor<CommentDto> descriptor)
    {
        descriptor.Field(f => f.Id).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.UserName).Type<StringType>();
        descriptor.Field(f => f.Text).Type<StringType>();
        descriptor.Field(f => f.ParentId).Type<IntType>();
        descriptor.Field(f => f.CreatedAt).Type<DateTimeType>();
        descriptor.Field(f => f.RepliesCount).Type<IntType>();
        descriptor.Field(f => f.Replies).Type<ListType<CommentDtoType>>(); 
    }
}
