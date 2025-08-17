using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Commands;

public record CreateColumnCommand(Guid UserId, string Name, Guid BoardId) : IRequest<Guid>, IBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Member]));
    }
}