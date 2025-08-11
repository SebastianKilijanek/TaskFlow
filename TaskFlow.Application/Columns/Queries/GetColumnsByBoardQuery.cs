using MediatR;
using TaskFlow.Application.Columns.DTO;

namespace TaskFlow.Application.Columns.Queries;

public record GetColumnsByBoardQuery(Guid BoardId) : IRequest<List<ColumnDTO>>;