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
    [HttpGet("{boardId}/users")]
    public async Task<IActionResult> GetUsersInBoard(Guid boardId)
    {
        var users = await mediator.Send(new GetUsersInBoardQuery(User.GetUserId(), boardId));
        return Ok(users);
    }
    
    [HttpPost("{boardId}/users")]
    public async Task<IActionResult> AddUserToBoard(Guid boardId, [FromBody] AddUserToBoardCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), BoardId = boardId });
        return NoContent();
    }

    [HttpDelete("{boardId}/users/{userIdToRemove}")]
    public async Task<IActionResult> RemoveUserFromBoard(Guid boardId, Guid userIdToRemove)
    {
        await mediator.Send(new RemoveUserFromBoardCommand(User.GetUserId(), boardId, userIdToRemove));
        return NoContent();
    }
    
    [HttpGet("{boardId}/users/{userIdToChange}")]
    public async Task<IActionResult> ChangeUserRole(Guid boardId, Guid userIdToChange, [FromBody] ChangeUserBoardRoleCommand command)
    {
        await mediator.Send(command with { UserId = User.GetUserId(), BoardId = boardId, UserIdToChange = userIdToChange});
        return NoContent();
    }

    [HttpPost("{boardId}/users/{userIdToTransfer}")]
    public async Task<IActionResult> TransferOwnership(Guid boardId, Guid userIdToTransfer)
    {
        await mediator.Send(new TransferBoardOwnershipCommand(User.GetUserId(), boardId, userIdToTransfer));
        return NoContent();
    }
}