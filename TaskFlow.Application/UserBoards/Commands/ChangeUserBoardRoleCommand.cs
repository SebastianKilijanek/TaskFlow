using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Commands;

public record ChangeUserBoardRoleCommand(Guid UserId, Guid BoardId,
    Guid UserIdToChange, BoardRole NewRole) : IRequest<Unit>, IUserBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner]));
    }
}