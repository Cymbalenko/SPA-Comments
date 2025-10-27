using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Models;


[Table("Files", Schema = "dbo")]
public class FileModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("FileId")]
    public int Id { get; set; }

    [Required]
    [Column("CommentId")]
    public int CommentId { get; set; }

    [Required]
    [Column("FileName")]
    public string Name { get; set; }

    [Required]
    [Column("ContentType")]
    public string ContentType { get; set; }

    [Required]
    [Column("FileUri")]
    public string Uri { get; set; }

    [Required]
    [Column("IsImage")]
    public bool IsImage { get; set; }

    [Required]
    [Column("UploadedAt")]
    public DateTime UploadedAt { get; set; }

    [ForeignKey("CommentId")]
    public virtual CommentModel Comment { get; set; }
}

