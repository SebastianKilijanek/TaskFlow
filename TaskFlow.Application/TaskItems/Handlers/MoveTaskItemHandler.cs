using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class MoveTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<MoveTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(MoveTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskToMove = request.TaskItem!;
        var oldColumnId = taskToMove.ColumnId;
        var newColumnId = request.NewColumnId;

        var newColumn = await unitOfWork.Repository<Column>().GetByIdAsync(newColumnId);
        if (newColumn is null)
        {
            throw new NotFoundException($"Destination column with ID '{newColumnId}' not found.");
        }

        if (newColumn.BoardId != taskToMove.Column.BoardId)
        {
            throw new BadRequestException("Cannot move tasks between different boards.");
        }

        // Case 1: Moving to a different column on the same board.
        if (oldColumnId != newColumnId)
        {
            // Reorder tasks in the old column.
            var oldColumnTasks = (await unitOfWork.Repository<TaskItem>()
                    .ListAsync(t => t.ColumnId == oldColumnId && t.Id != taskToMove.Id))
                .OrderBy(t => t.Position)
                .ToList();
            ReorderTasks(oldColumnTasks);

            // Move task and reorder tasks in the new column.
            var newColumnTasks = (await unitOfWork.Repository<TaskItem>()
                    .ListAsync(t => t.ColumnId == newColumnId))
                .OrderBy(t => t.Position)
                .ToList();

            var newPosition = Math.Clamp(request.NewPosition, 0, newColumnTasks.Count);
            newColumnTasks.Insert(newPosition, taskToMove);
            
            taskToMove.ColumnId = newColumnId;
            ReorderTasks(newColumnTasks);
        }
        // Case 2: Moving within the same column.
        else
        {
            var columnTasks = (await unitOfWork.Repository<TaskItem>()
                    .ListAsync(t => t.ColumnId == oldColumnId))
                .OrderBy(t => t.Position)
                .ToList();

            var taskIndex = columnTasks.FindIndex(t => t.Id == taskToMove.Id);
            
            // This should not happen if the task exists, but as a safeguard:
            if (taskIndex == -1) throw new ConflictException("Task to be moved not found in its own column list.");

            columnTasks.RemoveAt(taskIndex);

            var newPosition = Math.Clamp(request.NewPosition, 0, columnTasks.Count);
            columnTasks.Insert(newPosition, taskToMove);

            ReorderTasks(columnTasks);
        }

        await unitOfWork.SaveChangesAsync();
        return Unit.Value;
    }

    private void ReorderTasks(List<TaskItem> tasks)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            if (task.Position != i)
            {
                task.Position = i;
                unitOfWork.Repository<TaskItem>().Update(task);
            }
        }
    }
}