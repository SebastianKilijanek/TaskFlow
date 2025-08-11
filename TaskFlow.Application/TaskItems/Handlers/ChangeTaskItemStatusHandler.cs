using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class ChangeTaskItemStatusHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeTaskItemStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeTaskItemStatusCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.Id);
        if (taskItem == null) throw new Exception("TaskItem not found");

        taskItem.Status = (Enum.TryParse<TaskItemStatus>(request.Status, out var status) ? status
                            : throw new ArgumentException("Invalid status"));

        unitOfWork.Repository<TaskItem>().Update(taskItem);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}