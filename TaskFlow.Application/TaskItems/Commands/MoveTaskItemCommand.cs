using MediatR;
using TaskFlow.Application.TaskItems.Base;

namespace TaskFlow.Application.TaskItems.Commands;

public record MoveTaskItemCommand(Guid UserId, Guid Id, Guid NewColumnId, int NewPosition) : TaskItemRequestBase(UserId, Id), IRequest<Unit>;