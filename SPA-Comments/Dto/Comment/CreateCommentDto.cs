using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.Comment;

public class CreateCommentDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? HomePage { get; set; }
    public string CAPTCHA { get; set; }
    public string Text { get; set; }
    public int? ParentId { get; set; }
    public List<IFormFile>? Files { get; set; }
}
