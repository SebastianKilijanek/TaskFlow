using MediatR;
using TaskFlow.Application.TaskItems.Base;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.TaskItems.Commands;

public record UpdateTaskItemCommand(Guid UserId, Guid Id, string Title, string? Description, int Status, Guid? AssignedUserId) 
    : TaskItemRequestBase(UserId, Id), IRequest<Unit>;