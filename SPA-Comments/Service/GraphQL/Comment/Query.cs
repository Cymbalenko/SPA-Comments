using Dto.Comment;
using Service.Services.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.GraphQL.Comment;

public class Query
{
    private readonly ICommentService _service;

    public Query(ICommentService service)
    {
        _service = service;
    }
     
    public async Task<GetCommentListResponse> GetParentComments(
        int page = 1,
        int pageSize = 25,
        string sortField = "userName",
        string sort = "desc")
    {
        // переиспользуем существующий метод сервиса
        var res = await _service.GetParentCommentListAsync(page, pageSize, sort, sortField);
        return res;
    }
}
