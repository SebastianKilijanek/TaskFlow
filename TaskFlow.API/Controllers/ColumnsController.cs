using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Columns.DTO;
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
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnDTO dto, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateColumnCommand(User.GetUserId(), dto.Name, dto.BoardId), cancellationToken);
        return CreatedAtAction(nameof(GetColumn), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnDTO dto, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateColumnCommand(User.GetUserId(), id, dto.Name) , cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteColumn(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteColumnCommand(User.GetUserId(), id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> MoveColumn(Guid id, [FromBody] MoveColumnDTO dto, CancellationToken cancellationToken)
    {
        await mediator.Send(new MoveColumnCommand(User.GetUserId(), id, dto.NewPosition), cancellationToken);
        return NoContent();
    }
}