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
        var lastTaskItem = (await unitOfWork.Repository<TaskItem>()
                .ListAsync(t => t.ColumnId == request.ColumnId, cancellationToken))
                .OrderByDescending(t => t.Position)
                .FirstOrDefault();

        var newPosition = (lastTaskItem?.Position ?? -1) + 1;

        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ColumnId = request.ColumnId,
            Position = newPosition,
            Status = TaskItemStatus.ToDo,
            CreatedAt = DateTime.UtcNow,
            AssignedUserId = request.UserId
        };

        await unitOfWork.Repository<TaskItem>().AddAsync(taskItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return taskItem.Id;
    }
}