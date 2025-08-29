using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class DeleteBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteBoardCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        unitOfWork.Repository<Board>().Remove(request.Board);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}