using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class UpdateTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.Id);
        if (taskItem == null) throw new Exception("TaskItem not found");

        taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.Position = request.Position;
        taskItem.AssignedUserId = request.AssignedUserId;

        unitOfWork.Repository<TaskItem>().Update(taskItem);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}