using MediatR;
using TaskFlow.Application.TaskItems.DTO;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemByIdQuery(Guid Id) : IRequest<TaskItemDTO?>;