using MediatR;
using TaskFlow.Application.TaskItems.Base;

namespace TaskFlow.Application.TaskItems.Commands;

public record DeleteTaskItemCommand(Guid UserId, Guid Id) : TaskItemRequestBase(UserId, Id), IRequest<Unit>;