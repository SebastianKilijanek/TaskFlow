using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class DeleteTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.Id);
        if (taskItem == null) throw new Exception("TaskItem not found");

        unitOfWork.Repository<TaskItem>().Remove(taskItem);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}