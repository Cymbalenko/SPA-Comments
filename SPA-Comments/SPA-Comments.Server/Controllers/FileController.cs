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
    
    /// <summary>
    /// Получение файла (с SAS ссылкой)
    /// </summary>
    [HttpGet("download-url")]
    public async Task<IActionResult> Download([FromQuery] string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return BadRequest("Имя файла не указано.");

        var file = await _fileStorageService.GetFileAsync(blobName);
        if (file == null)
            return NotFound("Файл не найден.");

        // Добавляем заголовки для CORS
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Methods", "GET,OPTIONS");
        Response.Headers.Add("Access-Control-Allow-Headers", "*");

        // Возвращаем метаданные и ссылку (вместо потока)
        return Ok(new
        {
            file.FileName,
            file.ContentType,
            file.PublicUrl
        });
    }

    /// <summary>
    /// Получение публичной ссылки (если контейнер открыт)
    /// </summary>
    [HttpGet("public-url")]
    public async Task<IActionResult> GetPublicUrl([FromQuery] string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return BadRequest("Имя файла не указано.");

        var url = await _fileStorageService.GetPublicUrlAsync(blobName);
        if (url == null)
            return NotFound("Файл не найден.");

        return Ok(new { url });
    }
}
