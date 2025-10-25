using Dto.Azure;
using Dto.Comment;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Azure;
using Service.Services.Comment;

namespace SPA_Comments.Server.Controllers;
 
[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FileController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
    {
        var url = await _fileStorageService.UploadFileAsync(dto.File, "test-user");
        return Ok(new { url });
    }
}
