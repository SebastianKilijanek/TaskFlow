using MediatR;
using TaskFlow.Application.TaskItems.DTO;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemsByBoardQuery(Guid BoardId) : IRequest<IEnumerable<TaskItemDTO>>;