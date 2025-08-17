using MediatR;
using TaskFlow.Application.Columns.Base;

namespace TaskFlow.Application.Columns.Commands;

public record UpdateColumnCommand(Guid UserId, Guid Id, string Name) : ColumnRequestBase(UserId, Id), IRequest<Unit>;