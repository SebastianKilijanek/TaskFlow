using AutoMapper;
using MediatR;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class GetColumnsByBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetColumnsByBoardQuery, List<ColumnDTO>>
{
    public async Task<List<ColumnDTO>> Handle(GetColumnsByBoardQuery request, CancellationToken cancellationToken)
    {
        var columns = await unitOfWork.Repository<Column>().ListAsync(c => c.BoardId == request.BoardId);
        
        return columns.Select(mapper.Map<ColumnDTO>).ToList();
    }
}