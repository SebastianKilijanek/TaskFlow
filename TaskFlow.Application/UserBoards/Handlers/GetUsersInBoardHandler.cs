using AutoMapper;
using MediatR;
using TaskFlow.Application.UserBoards.DTO;
using TaskFlow.Application.UserBoards.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class GetUsersInBoardHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetUsersInBoardQuery, IReadOnlyList<UserBoardDTO>>
{
    public async Task<IReadOnlyList<UserBoardDTO>> Handle(GetUsersInBoardQuery request, CancellationToken cancellationToken)
    {
        var userBoards = await unitOfWork.Repository<UserBoard>()
            .ListAsync(ub => ub.BoardId == request.BoardId);

        return mapper.Map<IReadOnlyList<UserBoardDTO>>(userBoards);
    }
}