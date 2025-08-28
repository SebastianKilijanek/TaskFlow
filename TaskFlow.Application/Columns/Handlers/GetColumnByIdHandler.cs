using AutoMapper;
using MediatR;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class GetColumnByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetColumnByIdQuery, ColumnDTO?>
{
    public async Task<ColumnDTO?> Handle(GetColumnByIdQuery request, CancellationToken cancellationToken)
    {
        return mapper.Map<ColumnDTO>(request.Entity);
    }
}