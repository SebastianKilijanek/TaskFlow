using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Commands;

public record DeleteColumnCommand(Guid UserId, Guid Id) : IRequest<Unit>, IBoardAuthorizableRequest
{
    public Column? Column;
    
    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        Column = await unitOfWork.Repository<Column>().GetByIdAsync(Id);
        if (Column is null)
        {
            throw new NotFoundException($"Column with ID {Id} not found.");
        }

        return await Task.FromResult((Column.BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Member]));
    }
}