using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal.Models;

[Table("__EFMigrationsHistory")]
public class EFMigrationsHistory
{
    [Key]
    public string MigrationId { get; set; }
    public string ProductVersion { get; set; }
    public DateTime? DateTimeApply { get; set; }
    public string? VersionModel { get; set; }
}
