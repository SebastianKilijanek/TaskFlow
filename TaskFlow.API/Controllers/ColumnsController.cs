using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ColumnsController(IMediator mediator) : ControllerBase
{
    [HttpGet("board/{boardId:guid}")]
    public async Task<IActionResult> GetColumns(Guid boardId, CancellationToken cancellationToken)
    {
        var columns = await mediator.Send(new GetColumnsByBoardQuery(User.GetUserId(), boardId), cancellationToken);
        return Ok(columns);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetColumn(Guid id, CancellationToken cancellationToken)
    {
        var column = await mediator.Send(new GetColumnByIdQuery(User.GetUserId(), id), cancellationToken);
        return Ok(column);
    }

    [HttpPost]
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command with { UserId = User.GetUserId() }, cancellationToken);
        return CreatedAtAction(nameof(GetColumn), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteColumn(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteColumnCommand(User.GetUserId(), id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> MoveColumn(Guid id, [FromBody] MoveColumnCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id }, cancellationToken);
        return NoContent();
    }
}