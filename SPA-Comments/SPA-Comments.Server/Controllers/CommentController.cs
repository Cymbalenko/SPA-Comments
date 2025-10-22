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
    private readonly IValidator<CreateCommentDto> _createValidator;

    public CommentController(ICommentService service, IValidator<CreateCommentDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCommentAsync([FromForm] CreateCommentDto dto)
    {
        _createValidator.ValidateAndThrow(dto);
        var id = _service.CreateCommentAsync(dto);
        return Ok(id);
    }

    [HttpGet("create/{threadId}")]
    public async Task<IActionResult> GetParentCommentListAsync(int page = 1, int pageSize = 25, string sort = "desc")
    {
        var res = await _service.GetParentCommentListAsync(page, pageSize, sort);
        return Ok(res);
    }
}
