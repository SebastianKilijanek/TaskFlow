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
    public async Task<IActionResult> GetTaskItemsByColumn(Guid columnId)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByColumnQuery(User.GetUserId(), columnId));
        return Ok(taskItems);
    }

    [HttpGet("board/{boardId:guid}")]
    public async Task<IActionResult> GetTaskItemsByBoard(Guid boardId)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByBoardQuery(User.GetUserId(), boardId));
        return Ok(taskItems);
    }

    [HttpGet("user/{targetUserId:guid}/board/{boardId:guid}")]
    public async Task<IActionResult> GetTaskItemsByUser(Guid targetUserId, Guid boardId)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByUserQuery(User.GetUserId(), targetUserId, boardId));
        return Ok(taskItems);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTaskItem(Guid id)
    {
        var taskItem = await mediator.Send(new GetTaskItemByIdQuery(User.GetUserId(), id));
        return Ok(taskItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaskItem([FromBody] CreateTaskItemCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskItem), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTaskItem(Guid id, [FromBody] UpdateTaskItemCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTaskItem(Guid id)
    {
        await mediator.Send(new DeleteTaskItemCommand(User.GetUserId(),id));
        return NoContent();
    }

    [HttpPost("{id:guid}/move")]
    public async Task<IActionResult> MoveTaskItem(Guid id, [FromBody] MoveTaskItemCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id });
        return NoContent();
    }
}