using AutoMapper;
using MediatR;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers
{
    public class GetBoardsHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetBoardsQuery, IEnumerable<BoardDTO>>
    {
        public async Task<IEnumerable<BoardDTO>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
        {
            var boards = await unitOfWork.Repository<Board>().ListAsync();
            return boards.Select(mapper.Map<BoardDTO>);
        }
    }
}