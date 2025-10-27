using Dto.Comment;
using Service.Services.Comment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.GraphQL.Comment
{
    public class Query
    {
        private readonly ICommentService _service;

        public Query(ICommentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Получение списка корневых комментариев (без parentId)
        /// </summary>
        public async Task<GetCommentListResponse> GetParentComments(
            int page = 1,
            int pageSize = 25,
            string sortField = "userName",
            string sort = "desc")
        {
            return await _service.GetParentCommentListAsync(page, pageSize, sort, sortField);
        }

        /// <summary>
        /// Получение дочерних комментариев по parentId
        /// </summary>
        public async Task<List<CommentDto>> GetReplies(int parentId)
        {
            // Запрашиваем дочерние комментарии через сервис
            var replies = await _service.GetCommentsTreeAsync(parentId);
            return replies;
        }
    }
}
