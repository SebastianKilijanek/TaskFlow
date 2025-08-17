using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Commands;

public record UpdateBoardCommand(Guid UserId, Guid Id, string Name, bool IsPublic) : IRequest<Unit>, IBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((Id, (IEnumerable<BoardRole>)[BoardRole.Owner]));
    }
}