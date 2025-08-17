using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class DeleteBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteBoardCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await unitOfWork.Repository<Board>().GetByIdAsync(request.Id);
        unitOfWork.Repository<Board>().Remove(board!);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}