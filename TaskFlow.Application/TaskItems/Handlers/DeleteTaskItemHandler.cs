using MediatR;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class DeleteTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItemToDelete = request.Entity;
        var columnId = taskItemToDelete.ColumnId;

        // Fetch all tasks in the column to reorder them after deletion.
        var allTasksInColumn = (await unitOfWork.Repository<TaskItem>()
                .ListAsync(t => t.ColumnId == columnId))
                .OrderBy(t => t.Position)
                .ToList();

        unitOfWork.Repository<TaskItem>().Remove(taskItemToDelete);

        // Remove the deleted task from the in-memory list to get the correct remaining tasks.
        allTasksInColumn.RemoveAll(t => t.Id == taskItemToDelete.Id);

        // Reorder the positions of the remaining tasks.
        for (int i = 0; i < allTasksInColumn.Count; i++)
        {
            var task = allTasksInColumn[i];
            if (task.Position != i)
            {
                task.Position = i;
                unitOfWork.Repository<TaskItem>().Update(task);
            }
        }

        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}