using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.DTO;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Queries;

public record GetUsersInBoardQuery(Guid UserId, Guid BoardId) : IRequest<IReadOnlyList<UserBoardDTO>>, IUserBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Member, BoardRole.Viewer]));
    }
}