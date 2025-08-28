using MediatR;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Handlers;

public class UpdateBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateBoardCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        request.Board!.Name = request.Name;
        request.Board.IsPublic = request.IsPublic;

        unitOfWork.Repository<Board>().Update(request.Board);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}