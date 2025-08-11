using MediatR;

namespace TaskFlow.Application.TaskItems.Commands;

public record UpdateTaskItemCommand(Guid Id, string Title, string? Description, int Position, Guid? AssignedUserId) : IRequest<Unit>;