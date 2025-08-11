using MediatR;

namespace TaskFlow.Application.TaskItems.Commands;

public record MoveTaskItemCommand(Guid Id, Guid NewColumnId, int NewPosition) : IRequest<Unit>;