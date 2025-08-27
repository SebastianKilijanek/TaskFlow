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
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await mediator.Send(new GetUserByIdQuery(User.GetUserId()));
        return Ok(user);
    }

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordCommand request)
    {
        await mediator.Send(request with { UserId = User.GetUserId() });
        return NoContent();
    }

    [HttpPut("me/change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeUserEmailCommand request)
    {
        await mediator.Send(request with { UserId = User.GetUserId() });
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id));
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        await mediator.Send(command with { UserId = id });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}