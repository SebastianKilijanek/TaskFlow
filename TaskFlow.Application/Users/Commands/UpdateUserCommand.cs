using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Users.Commands;

public record UpdateUserCommand(Guid UserId, string Email, string UserName, string Role)
    : IRequest<Unit>, IUserEntityRequest
{
    public User Entity { get; set; } = null!;
}