using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Models;

[Table("Comments", Schema = "dbo")]
public class CommentModel
{
    public CommentModel()
    {
        Replies = new HashSet<CommentModel>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CommentId")]
    public int Id { get; set; }

    [Required]
    [Column("UserId")]
    public int UserId { get; set; }

    [Required]
    [Column("CAPTCHA")]
    public string CAPTCHA { get; set; }

    [Required]
    [Column("Text")]
    public string Text { get; set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("ParentCommentId")]
    public int? ParentId { get; set; }

    [ForeignKey("UserId")]
    public virtual UserModel User { get; set; }

    [ForeignKey("ParentCommentId")]
    public virtual CommentModel? ParentComment { get; set; }

    [InverseProperty(nameof(ParentComment))]
    public virtual ICollection<CommentModel> Replies { get; set; }
}
