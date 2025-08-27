using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Users.Commands;

public record UpdateUserCommand(Guid UserId, string Email, string UserName, string Role) : IRequest<Unit>, IUserExistenceRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
}