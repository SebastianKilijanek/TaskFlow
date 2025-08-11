using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class MoveTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<MoveTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(MoveTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.Id);
        if (taskItem == null) throw new Exception("TaskItem not found");

        taskItem.ColumnId = request.NewColumnId;
        taskItem.Position = request.NewPosition;

        unitOfWork.Repository<TaskItem>().Update(taskItem);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}