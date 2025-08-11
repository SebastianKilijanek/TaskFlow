using AutoMapper;
using MediatR;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class GetColumnByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetColumnByIdQuery, ColumnDTO?>
{
    public async Task<ColumnDTO?> Handle(GetColumnByIdQuery request, CancellationToken cancellationToken)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(request.Id);
        
        return column is null ? null : mapper.Map<ColumnDTO>(column);
    }
}