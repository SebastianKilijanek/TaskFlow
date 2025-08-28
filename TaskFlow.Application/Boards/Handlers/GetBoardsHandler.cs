using AutoMapper;
using MediatR;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class GetBoardsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetBoardsQuery, IReadOnlyList<BoardDTO>>
{
    public async Task<IReadOnlyList<BoardDTO>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var userBoards = await unitOfWork.Repository<UserBoard>()
            .ListAsync(ub => ub.UserId == request.UserId);
        var boardIds = userBoards.Select(ub => ub.BoardId).ToList();

        if (!boardIds.Any())
        {
            return [];
        }

        var boards = await unitOfWork.Repository<Board>()
            .ListAsync(b => boardIds.Contains(b.Id));

        return mapper.Map<IReadOnlyList<BoardDTO>>(boards);
    }
}