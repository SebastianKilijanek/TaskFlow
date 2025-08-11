using AutoMapper;
using MediatR;
using TaskFlow.Application.TaskItems.Queries;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class GetTaskItemsByBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetTaskItemsByBoardQuery, IEnumerable<TaskItemDTO>>
{
    public async Task<IEnumerable<TaskItemDTO>> Handle(GetTaskItemsByBoardQuery request, CancellationToken cancellationToken)
    {
        var columns = await unitOfWork.Repository<Column>().ListAsync(c => c.BoardId == request.BoardId);
        var columnIds = columns.Select(c => c.Id).ToList();

        var taskItems = await unitOfWork.Repository<TaskItem>().ListAsync(t => columnIds.Contains(t.ColumnId));
        
        return taskItems.Select(mapper.Map<TaskItemDTO>)
                        .OrderBy(t => t.ColumnId)
                        .ThenBy(t => t.Position);
    }
}