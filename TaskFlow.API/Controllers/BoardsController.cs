using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Queries;

[ApiController]
[Route("api/[controller]")]
public class BoardsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var boards = await mediator.Send(new GetBoardsQuery());
        return Ok(boards);
    }
}