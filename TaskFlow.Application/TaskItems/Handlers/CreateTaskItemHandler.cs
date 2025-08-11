using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class CreateTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateTaskItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ColumnId = request.ColumnId,
            Position = request.Position,
            Status = TaskItemStatus.ToDo,
            CreatedAt = DateTime.UtcNow,
        };
        
        await unitOfWork.Repository<TaskItem>().AddAsync(taskItem);
        await unitOfWork.SaveChangesAsync();
        
        return taskItem.Id;
    }
}