using AutoMapper;
using MediatR;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Application.Columns.Queries;

namespace TaskFlow.Application.Columns.Handlers;

public class GetColumnByIdHandler(IMapper mapper) : IRequestHandler<GetColumnByIdQuery, ColumnDTO?>
{
    public Task<ColumnDTO> Handle(GetColumnByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(mapper.Map<ColumnDTO>(request.Entity));
    }
}