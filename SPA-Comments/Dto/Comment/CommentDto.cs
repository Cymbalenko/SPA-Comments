using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.Comment;

public class CommentDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? HomePage { get; set; }
    public string CAPTCHA { get; set; }
    public string Text { get; set; }
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RepliesCount { get; set; }
    public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    public List<CommentFileDto> Files { get; set; } = new List<CommentFileDto>();
}
