using AutoMapper;
using MediatR;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTaskItemByIdQuery, TaskItemDTO?>
{
    public async Task<TaskItemDTO?> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
    {
        var taskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.Id);
        
        return taskItem is null ? null : mapper.Map<TaskItemDTO>(taskItem);
    }
}