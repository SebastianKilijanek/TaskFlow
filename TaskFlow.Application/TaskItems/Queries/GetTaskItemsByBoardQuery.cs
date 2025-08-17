using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemsByBoardQuery(Guid UserId, Guid BoardId) : IRequest<IReadOnlyList<TaskItemDTO>>, IUserBoardAuthorizableRequest
{
    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return await Task.FromResult((BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer]));
    }
}