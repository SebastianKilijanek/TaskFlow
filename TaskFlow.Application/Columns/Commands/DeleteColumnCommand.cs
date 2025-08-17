using MediatR;
using TaskFlow.Application.Columns.Base;

namespace TaskFlow.Application.Columns.Commands;

public record DeleteColumnCommand(Guid UserId, Guid Id) : ColumnRequestBase(UserId, Id), IRequest<Unit>;