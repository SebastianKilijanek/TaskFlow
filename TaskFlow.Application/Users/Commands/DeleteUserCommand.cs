using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Unit>, IEntityExistenceRequest<User>
{
    public Guid EntityId => Id;
    public User Entity { get; set; }
}