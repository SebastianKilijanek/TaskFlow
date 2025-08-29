using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Application.Users.Queries;

namespace TaskFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(User.GetUserId()), cancellationToken);
        return Ok(user);
    }

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request with { UserId = User.GetUserId() }, cancellationToken);
        return NoContent();
    }

    [HttpPut("me/change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeUserEmailCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request with { UserId = User.GetUserId() }, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { UserId = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}