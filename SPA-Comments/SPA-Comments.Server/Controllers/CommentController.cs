using Dto.Comment;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Comment;

namespace SPA_Comments.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentController(ICommentService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCommentAsync([FromForm] CreateCommentDto dto)
    {
        var id = await _service.CreateCommentAsync(dto);
        return Ok(id);
    }

    [HttpGet("getCommentList")]
    public async Task<IActionResult> GetParentCommentListAsync([FromQuery] int page = 1,[FromQuery] string sortField = "userName", [FromQuery] int pageSize = 25, [FromQuery] string sort = "desc")
    {
        var res = await _service.GetParentCommentListAsync(page, pageSize, sort, sortField);
        return Ok(res);
    }
}
