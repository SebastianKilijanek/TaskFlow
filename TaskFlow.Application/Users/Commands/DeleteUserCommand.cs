using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Unit>, IUserEntityRequest
{
    public User Entity { get; set; } = null!;
    public Guid UserId => Id;
}