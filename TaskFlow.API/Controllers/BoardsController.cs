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
        var boards = await mediator.Send(new GetBoardsQuery(User.GetUserId()));
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await mediator.Send(new GetBoardByIdQuery(User.GetUserId(), id));
        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardCommand command)
    {
        var id = await mediator.Send(command with { UserId = User.GetUserId() });
        return CreatedAtAction(nameof(GetBoard), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), Id = id });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        await mediator.Send(new DeleteBoardCommand(User.GetUserId(), id));
        return NoContent();
    }
}