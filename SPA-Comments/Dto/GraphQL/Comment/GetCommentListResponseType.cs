using Dto.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.GraphQL.Comment;

public class GetCommentListResponseType : ObjectType<GetCommentListResponse>
{
    protected override void Configure(IObjectTypeDescriptor<GetCommentListResponse> descriptor)
    {
        descriptor.Field(f => f.Page).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.PageSize).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.TotalCount).Type<NonNullType<IntType>>();
        descriptor.Field(f => f.Data).Type<ListType<CommentDtoType>>();
    }
}
