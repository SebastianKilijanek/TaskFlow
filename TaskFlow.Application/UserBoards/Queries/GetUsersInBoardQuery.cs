using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Queries;

public record GetUsersInBoardQuery(Guid UserId, Guid BoardId) 
    : IRequest<IReadOnlyList<UserBoardDTO>>, IUserExistenceRequest, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;

    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer]));
    }
}