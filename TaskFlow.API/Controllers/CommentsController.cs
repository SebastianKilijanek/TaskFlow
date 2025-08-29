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
    public async Task<IActionResult> GetCommentsForTask(Guid taskItemId, CancellationToken cancellationToken)
    {
        var comments = await mediator.Send(new GetCommentsForTaskQuery(User.GetUserId(), taskItemId), cancellationToken);
        return Ok(comments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComment(Guid id, CancellationToken cancellationToken)
    {
        var comment = await mediator.Send(new GetCommentByIdQuery(User.GetUserId(), id), cancellationToken);
        return Ok(comment);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] AddCommentCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command with { UserId = User.GetUserId() }, cancellationToken);
        return CreatedAtAction(nameof(GetComment), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCommentCommand(User.GetUserId(), id), cancellationToken);
        return NoContent();
    }
}