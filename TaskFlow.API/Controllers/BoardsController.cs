using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Boards.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BoardsController(IMediator mediator) : ControllerBase
{
    // v1 endpoint
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetBoards(CancellationToken cancellationToken)
    {
        var boards = await mediator.Send(new GetBoardsQuery(User.GetUserId()), cancellationToken);
        return Ok(boards);
    }

    // v2 endpoint with pagination
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetBoardsV2([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var boards = await mediator.Send(new GetBoardsQuery(User.GetUserId()), cancellationToken);
        var pagedBoards = boards.Skip((page - 1) * pageSize).Take(pageSize);
        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = boards.Count(),
            Items = pagedBoards
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBoard(Guid id, CancellationToken cancellationToken)
    {
        var board = await mediator.Send(new GetBoardByIdQuery(User.GetUserId(), id), cancellationToken);
        return Ok(board);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDTO dto, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateBoardCommand( User.GetUserId(), dto.Name, dto.IsPublic), cancellationToken);
        return CreatedAtAction(nameof(GetBoard), new { id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDTO dto, CancellationToken cancellationToken)
    {    
        await mediator.Send(new UpdateBoardCommand(User.GetUserId(), id, dto.Name, dto.IsPublic), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBoard(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteBoardCommand(User.GetUserId(), id), cancellationToken);
        return NoContent();
    }
}