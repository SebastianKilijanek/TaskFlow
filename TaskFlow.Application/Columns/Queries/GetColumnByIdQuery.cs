using MediatR;
using TaskFlow.Application.Columns.Base;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Columns.Queries;

public record GetColumnByIdQuery(Guid UserId, Guid Id) : ColumnRequestBase(UserId, Id), IRequest<ColumnDTO>
{
    protected override IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer];
}