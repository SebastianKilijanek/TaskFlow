using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemsByColumnHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetTaskItemsByColumnQuery, IReadOnlyList<TaskItemDTO>>
{
    public async Task<IReadOnlyList<TaskItemDTO>> Handle(GetTaskItemsByColumnQuery request, CancellationToken cancellationToken)
    {
        var taskItems = await unitOfWork.Repository<TaskItem>().ListAsync(t => t.ColumnId == request.ColumnId, cancellationToken);

        return taskItems.Select(mapper.Map<TaskItemDTO>)
            .OrderBy(t => t.Position)
            .ToList();
    }
}