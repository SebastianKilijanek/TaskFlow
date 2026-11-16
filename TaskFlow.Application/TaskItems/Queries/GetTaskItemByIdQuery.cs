using MediatR;
using TaskFlow.Application.TaskItems.Base;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemByIdQuery(Guid UserId, Guid Id) : TaskItemRequestBase(UserId, Id), IRequest<TaskItemDTO>
{
    public bool AllowPublicAccess => true;

    protected override IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer];
}