using MediatR;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Queries;

public record GetColumnsByBoardQuery(Guid UserId, Guid BoardId) : IRequest<IReadOnlyList<ColumnDTO>>, IUserBoardAuthorizableRequest
{
    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Member, BoardRole.Viewer]));
    }
}