using MediatR;
using TaskFlow.Application.Columns.DTO;

namespace TaskFlow.Application.Columns.Queries;

public record GetColumnByIdQuery(Guid Id) : IRequest<ColumnDTO?>;