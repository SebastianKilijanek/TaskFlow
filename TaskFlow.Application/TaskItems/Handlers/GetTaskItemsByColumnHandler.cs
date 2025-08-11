using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemsByColumnHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetTaskItemsByColumnQuery, IEnumerable<TaskItemDTO>>
{
    public async Task<IEnumerable<TaskItemDTO>> Handle(GetTaskItemsByColumnQuery request, CancellationToken cancellationToken)
    {
        var taskItems = await unitOfWork.Repository<TaskItem>().ListAsync(t => t.ColumnId == request.ColumnId);
        
        return taskItems.Select(mapper.Map<TaskItemDTO>);
    }
}