using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Base;

public abstract record TaskItemRequestBase(Guid userId, Guid Id) : IUserBoardAuthorizableRequest
{
    public Guid UserId { get; set; } = userId;
    public Guid Id { get; set; } = Id;
    public TaskItem? TaskItem { get; protected set; }
    protected virtual IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Member];

    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        TaskItem = await unitOfWork.Repository<TaskItem>().GetByIdAsync(Id);
        if (TaskItem == null)
        {
            throw new NotFoundException($"TaskItem with id '{Id}' not found.");
        }

        return (TaskItem.Column.BoardId, RequiredRoles);
    }
}