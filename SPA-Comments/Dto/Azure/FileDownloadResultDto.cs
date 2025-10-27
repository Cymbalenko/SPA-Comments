using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto.Azure;

public class FileDownloadResultDto
{
    public Stream Stream { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string PublicUrl { get; set; } = default!;
}
