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
    [HttpGet("board/{boardId}")]
    public async Task<IActionResult> GetColumns(Guid boardId)
    {
        var columns = await mediator.Send(new GetColumnsByBoardQuery(boardId));
        return Ok(columns);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetColumn(Guid id)
    {
        var column = await mediator.Send(new GetColumnByIdQuery(id));
        if (column == null) return NotFound();
        return Ok(column);
    }

    [HttpPost]
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetColumn), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnCommand command)
    {
        await mediator.Send(command with { Id = id });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        await mediator.Send(new DeleteColumnCommand(id));
        return NoContent();
    }
}