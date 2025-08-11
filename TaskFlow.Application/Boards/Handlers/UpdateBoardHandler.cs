using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class UpdateBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateBoardCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await unitOfWork.Repository<Board>().GetByIdAsync(request.Id);
        if (board == null) throw new Exception("Board not found");

        board.Name = request.Name;
        board.IsPublic = request.IsPublic;

        unitOfWork.Repository<Board>().Update(board);
        await unitOfWork.SaveChangesAsync();
        return Unit.Value;
    }
}