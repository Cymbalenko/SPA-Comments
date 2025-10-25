using Dto.Captcha;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Captcha;
using Service.Services.Comment;

namespace SPA_Comments.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    private readonly ICaptchaService _service;
    public CaptchaController(ICaptchaService service)
    {
        _service = service;
    }

    [HttpGet("image")]
    public IActionResult GetCaptchaImage()
    {
        var (imageBytes, text) = _service.GenerateCaptcha();
        HttpContext.Session.SetString("CaptchaCode", text);

        return File(imageBytes, "image/png");
    }

    [HttpPost("validate")]
    public IActionResult ValidateCaptcha([FromBody] CaptchaCheckDto dto)
    {
        var stored = HttpContext.Session.GetString("CaptchaCode");
        if (string.IsNullOrEmpty(stored))
            return BadRequest("Captcha expired");

        if (stored.Equals(dto.Value, StringComparison.OrdinalIgnoreCase))
            return Ok(true);

        return BadRequest("Invalid captcha");
    }
}
