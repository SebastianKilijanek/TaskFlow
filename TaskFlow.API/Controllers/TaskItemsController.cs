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
    [HttpGet("column/{columnId}")]
    public async Task<IActionResult> GetTaskItemsByColumn(Guid columnId)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByColumnQuery(columnId));
        return Ok(taskItems);
    }

    [HttpGet("board/{boardId}")]
    public async Task<IActionResult> GetTaskItemsByBoard(Guid boardId)
    {
        var taskItems = await mediator.Send(new GetTaskItemsByBoardQuery(boardId));
        return Ok(taskItems);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserTaskItems(Guid userId)
    {
        var taskItems = await mediator.Send(new GetUserTaskItemsQuery(userId));
        return Ok(taskItems);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskItem(Guid id)
    {
        var taskItem = await mediator.Send(new GetTaskItemByIdQuery(id));
        if (taskItem == null) return NotFound();
        return Ok(taskItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaskItem([FromBody] CreateTaskItemCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskItem), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTaskItem(Guid id, [FromBody] UpdateTaskItemCommand command)
    {
        var cmd = command with { Id = id };
        await mediator.Send(cmd);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskItem(Guid id)
    {
        await mediator.Send(new DeleteTaskItemCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/move")]
    public async Task<IActionResult> MoveTaskItem(Guid id, [FromBody] MoveTaskItemCommand command)
    {
        var cmd = command with { Id = id };
        await mediator.Send(cmd);
        return NoContent();
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> ChangeTaskItemStatus(Guid id, [FromBody] ChangeTaskItemStatusCommand command)
    {
        var cmd = command with { Id = id };
        await mediator.Send(cmd);
        return NoContent();
    }
}