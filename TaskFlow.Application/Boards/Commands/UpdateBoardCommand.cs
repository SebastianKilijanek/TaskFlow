using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Commands;

public record UpdateBoardCommand(Guid UserId, Guid Id, string Name, bool IsPublic) : IRequest<Unit>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;
    
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((Id, (IEnumerable<BoardRole>)[BoardRole.Owner]));
    }
}