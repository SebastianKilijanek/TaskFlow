using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetUserTaskItemsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUserTaskItemsQuery, IEnumerable<TaskItemDTO>>
{
    public async Task<IEnumerable<TaskItemDTO>> Handle(GetUserTaskItemsQuery request, CancellationToken cancellationToken)
    {
        var taskItems = await unitOfWork.Repository<TaskItem>().ListAsync(t => t.AssignedUserId == request.UserId);
        
        return taskItems.Select(mapper.Map<TaskItemDTO>);
    }
}