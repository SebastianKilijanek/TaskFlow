using AutoMapper;
using MediatR;
using System.Linq.Expressions;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class GetBoardsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetBoardsQuery, PagedResult<BoardDTO>>
{
    public async Task<PagedResult<BoardDTO>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var userBoards = await unitOfWork.Repository<UserBoard>()
            .ListReadOnlyAsync(ub => ub.UserId == request.UserId, cancellationToken);
        var boardIds = userBoards.Select(ub => ub.BoardId).ToList();

        if (boardIds.Count == 0)
            return new PagedResult<BoardDTO>([], page, pageSize, 0);

        Expression<Func<Board, bool>> predicate = b => boardIds.Contains(b.Id);

        var totalCount = await unitOfWork.Repository<Board>().CountAsync(predicate, cancellationToken);
        var boards = await unitOfWork.Repository<Board>().ListPagedReadOnlyAsync(predicate, page, pageSize, cancellationToken);

        return new PagedResult<BoardDTO>(mapper.Map<List<BoardDTO>>(boards), page, pageSize, totalCount);
    }
}