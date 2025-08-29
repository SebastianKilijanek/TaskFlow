using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Application.UserBoards.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/boards")]
public class UserBoardController(IMediator mediator) : ControllerBase
{
    [HttpGet("{boardId:guid}/users")]
    public async Task<IActionResult> GetUsersInBoard(Guid boardId, CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetUsersInBoardQuery(User.GetUserId(), boardId), cancellationToken);
        return Ok(users);
    }
    
    [HttpPost("{boardId:guid}/users")]
    public async Task<IActionResult> AddUserToBoard(Guid boardId, [FromBody] AddUserToBoardCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), BoardId = boardId }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{boardId:guid}/users/{userIdToRemove:guid}")]
    public async Task<IActionResult> RemoveUserFromBoard(Guid boardId, Guid userIdToRemove, CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveUserFromBoardCommand(User.GetUserId(), boardId, userIdToRemove), cancellationToken);
        return NoContent();
    }
    
    [HttpPut("{boardId:guid}/users/{userIdToChange:guid}")]
    public async Task<IActionResult> ChangeUserRole(Guid boardId, Guid userIdToChange, [FromBody] ChangeUserBoardRoleCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), BoardId = boardId, UserIdToChange = userIdToChange}, cancellationToken);
        return NoContent();
    }

    [HttpPost("{boardId:guid}/users/{userIdToTransfer:guid}")]
    public async Task<IActionResult> TransferOwnership(Guid boardId, Guid userIdToTransfer, CancellationToken cancellationToken)
    {
        await mediator.Send(new TransferBoardOwnershipCommand(User.GetUserId(), boardId, userIdToTransfer), cancellationToken);
        return NoContent();
    }
}