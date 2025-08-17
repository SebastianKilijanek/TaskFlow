using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class UpdateBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateBoardCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var boardRep = unitOfWork.Repository<Board>();
        var board = await boardRep.GetByIdAsync(request.Id);

        board!.Name = request.Name;
        board.IsPublic = request.IsPublic;

        boardRep.Update(board);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}