using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await mediator.Send(new GetBoardsQuery());
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await mediator.Send(new GetBoardByIdQuery(Id: id));
        if (board == null) return NotFound();
        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetBoard), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardCommand command)
    {
        await mediator.Send(command with { Id = id });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        await mediator.Send(new DeleteBoardCommand(id));
        return NoContent();
    }
}