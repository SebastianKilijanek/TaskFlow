using MediatR;

namespace TaskFlow.Application.TaskItems.Commands;

public record ChangeTaskItemStatusCommand(Guid Id, string Status) : IRequest<Unit>;