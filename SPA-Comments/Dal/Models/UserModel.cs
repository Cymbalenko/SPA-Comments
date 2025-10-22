using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Models;

[Table("Users", Schema = "dbo")]
public class UserModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("UserId")]
    public int Id { get; set; }

    [Required]
    [Column("UserName")]
    public string UserName { get; set; }

    [Required]
    [Column("Email")]
    public string Email { get; set; }

    [Column("HomePage")]
    public string? HomePage { get; set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    public virtual List<CommentModel> Comments { get; set; }
}
