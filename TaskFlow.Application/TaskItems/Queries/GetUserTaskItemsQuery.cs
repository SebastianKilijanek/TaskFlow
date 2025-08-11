using MediatR;
using TaskFlow.Application.TaskItems.DTO;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetUserTaskItemsQuery(Guid UserId) : IRequest<IEnumerable<TaskItemDTO>>;