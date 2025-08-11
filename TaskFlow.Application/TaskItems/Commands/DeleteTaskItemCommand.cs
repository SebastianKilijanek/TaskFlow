using MediatR;

namespace TaskFlow.Application.TaskItems.Commands;

public record DeleteTaskItemCommand(Guid Id) : IRequest<Unit>;