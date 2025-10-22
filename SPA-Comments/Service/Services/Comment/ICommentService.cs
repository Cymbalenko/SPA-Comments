using Dto.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Comment;

public interface ICommentService
{
    public Task<int> CreateCommentAsync(CreateCommentDto dto);

    public Task<GetCommentListResponse> GetParentCommentListAsync(int page = 1, int pageSize = 25, string sort = "desc");
}
