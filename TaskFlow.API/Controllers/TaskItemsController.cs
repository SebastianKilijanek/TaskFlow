using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Application.TaskItems.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet("column/{columnId:guid}")]
    public async Task<IActionResult> GetTaskItemsByColumn(Guid columnId, CancellationToken cancellationToken)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByColumnQuery(User.GetUserId(), columnId), cancellationToken);
        return Ok(taskItems);
    }

    [HttpGet("board/{boardId:guid}")]
    public async Task<IActionResult> GetTaskItemsByBoard(Guid boardId, CancellationToken cancellationToken)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByBoardQuery(User.GetUserId(), boardId), cancellationToken);
        return Ok(taskItems);
    }

    [HttpGet("user/{targetUserId:guid}/board/{boardId:guid}")]
    public async Task<IActionResult> GetTaskItemsByUser(Guid targetUserId, Guid boardId, CancellationToken cancellationToken)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByUserQuery(User.GetUserId(), targetUserId, boardId), cancellationToken);
        return Ok(taskItems);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTaskItem(Guid id, CancellationToken cancellationToken)
    {
        var taskItem = await mediator.Send(new GetTaskItemByIdQuery(User.GetUserId(), id), cancellationToken);
        return Ok(taskItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaskItem([FromBody] CreateTaskItemCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTaskItem), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTaskItem(Guid id, [FromBody] UpdateTaskItemCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTaskItem(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTaskItemCommand(User.GetUserId(),id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/move")]
    public async Task<IActionResult> MoveTaskItem(Guid id, [FromBody] MoveTaskItemCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id }, cancellationToken);
        return NoContent();
    }
}