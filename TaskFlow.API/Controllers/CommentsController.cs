using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Comments.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CommentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("taskitem/{taskItemId:guid}")]
    public async Task<IActionResult> GetCommentsForTask(Guid taskItemId)
    {
        var comments = await mediator.Send(new GetCommentsForTaskQuery(User.GetUserId(), taskItemId));
        return Ok(comments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComment(Guid id)
    {
        var comment = await mediator.Send(new GetCommentByIdQuery(User.GetUserId(), id));
        return Ok(comment);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentCommand command)
    {
        var id = await mediator.Send(command with { UserId = User.GetUserId() });
        return CreatedAtAction(nameof(GetComment), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await mediator.Send(new DeleteCommentCommand(User.GetUserId(), id));
        return NoContent();
    }
}