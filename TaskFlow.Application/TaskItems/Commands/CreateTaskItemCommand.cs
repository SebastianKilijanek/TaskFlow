using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Commands;

public record CreateTaskItemCommand(Guid UserId, string Title, string? Description, Guid ColumnId) 
    : IRequest<Guid>, IUserBoardAuthorizableRequest
{
    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(ColumnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {ColumnId} not found.");
        }
        
        return await Task.FromResult((column.BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor]));
    }
}