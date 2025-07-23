using MediatR;
using TaskFlow.Application.Boards.Queries;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers
{
    public class GetBoardByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBoardByIdQuery, BoardDTO?>
    {
        public async Task<BoardDTO?> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
        {
            var board = await unitOfWork.Repository<Board>().GetByIdAsync(request.Id);
            return board is null ? null : new BoardDTO(board.Id, board.Name, board.IsPublic);
        }
    }
}