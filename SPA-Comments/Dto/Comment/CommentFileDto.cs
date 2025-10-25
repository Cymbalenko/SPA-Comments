using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.Comment;

public class CommentFileDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public string ContentType { get; set; }

    public string Uri { get; set; }

    public bool IsImage { get; set; }

    public DateTime UploadedAt { get; set; }
}
