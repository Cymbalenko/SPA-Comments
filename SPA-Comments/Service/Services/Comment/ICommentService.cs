using Dto.Comment;
using HotChocolate.Data.Sorting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Comment;

public interface ICommentService
{
    public Task<int> CreateCommentAsync(CreateCommentDto dto);

    public Task<GetCommentListResponse> GetParentCommentListAsync(int page, int pageSize, string sort, string sortField);
}
