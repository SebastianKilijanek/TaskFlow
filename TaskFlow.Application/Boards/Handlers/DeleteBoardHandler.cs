using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class DeleteBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteBoardCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await unitOfWork.Repository<Domain.Entities.Board>().GetByIdAsync(request.Id);
        if (board == null) throw new Exception("Board not found");

        unitOfWork.Repository<Domain.Entities.Board>().Remove(board);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}