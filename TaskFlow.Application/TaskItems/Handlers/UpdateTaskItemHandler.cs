using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.TaskItems.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Handlers;

public class UpdateTaskItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTaskItemCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var taskItem = request.TaskItem!;

        if (request.AssignedUserId.HasValue && request.AssignedUserId != taskItem.AssignedUserId)
        {
            var boardId = taskItem.Column.BoardId;
            var userBoard = await unitOfWork.Repository<UserBoard>().GetByIdAsync(request.AssignedUserId.Value, boardId);
            if (userBoard == null)
            {
                throw new BadRequestException($"User with ID '{request.AssignedUserId}' is not a member of this board and cannot be assigned to the task.");
            }
        }
        
        if (!Enum.IsDefined(typeof(TaskItemStatus), request.Status))
        {
            throw new BadRequestException($"Invalid status value: '{request.Status}'.");
        }

        taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.Status = (TaskItemStatus)request.Status;
        taskItem.AssignedUserId = request.AssignedUserId;

        unitOfWork.Repository<TaskItem>().Update(taskItem);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}