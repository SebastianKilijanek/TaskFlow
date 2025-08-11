using MediatR;
using TaskFlow.Application.TaskItems.DTO;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemsByColumnQuery(Guid ColumnId) : IRequest<IEnumerable<TaskItemDTO>>;