using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemsByColumnQuery(Guid UserId, Guid ColumnId) 
    : IRequest<IReadOnlyList<TaskItemDTO>>, IUserBoardAuthorizableRequest
{
    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(ColumnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {ColumnId} not found.");
        }
        
        return await Task.FromResult((column.BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Member, BoardRole.Viewer]));
    }
}