using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Users.DTO;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDTO>, IEntityExistenceRequest<User>
{
    public Guid EntityId => UserId;
    public User Entity { get; set; }
}