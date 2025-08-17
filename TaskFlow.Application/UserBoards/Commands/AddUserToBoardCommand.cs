using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Commands;

public record AddUserToBoardCommand(Guid UserId, Guid BoardId, string UserEmail, BoardRole BoardRole) : IRequest<Unit>, IUserBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner]));
    }
}