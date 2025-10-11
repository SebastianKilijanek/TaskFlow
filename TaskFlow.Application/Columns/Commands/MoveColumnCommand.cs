using MediatR;
using TaskFlow.Application.Columns.Base;

namespace TaskFlow.Application.Columns.Commands;

public record MoveColumnCommand(Guid UserId, Guid Id, int NewPosition) : ColumnRequestBase(UserId, Id), IRequest<Unit>;