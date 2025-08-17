using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Base;

public abstract record ColumnRequestBase(Guid UserId, Guid Id) : IUserBoardAuthorizableRequest
{
    public Column? Column { get; protected set; }
    protected virtual IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor];

    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        Column = await unitOfWork.Repository<Column>().GetByIdAsync(Id);
        if (Column == null)
        {
            throw new NotFoundException($"Column with id '{Id}' not found.");
        }

        return (Column.BoardId, RequiredRoles);
    }
}