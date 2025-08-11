using MediatR;

namespace TaskFlow.Application.TaskItems.Commands;

public record CreateTaskItemCommand(string Title, string? Description, Guid ColumnId, int Position) : IRequest<Guid>;