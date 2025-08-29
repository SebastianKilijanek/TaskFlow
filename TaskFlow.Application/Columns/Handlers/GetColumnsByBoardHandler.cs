using AutoMapper;
using MediatR;
using TaskFlow.Application.Columns.Queries;
using TaskFlow.Application.Columns.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class GetColumnsByBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetColumnsByBoardQuery, IReadOnlyList<ColumnDTO>>
{
    public async Task<IReadOnlyList<ColumnDTO>> Handle(GetColumnsByBoardQuery request, CancellationToken cancellationToken)
    {
        var columns = await unitOfWork.Repository<Column>().ListAsync(c => c.BoardId == request.BoardId, cancellationToken);
        
        return columns
            .OrderBy(c => c.Position)
            .Select(mapper.Map<ColumnDTO>)
            .ToList();
    }
}