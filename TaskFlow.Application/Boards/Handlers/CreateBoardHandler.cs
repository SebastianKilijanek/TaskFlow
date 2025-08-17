using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Boards.Handlers;

public class CreateBoardHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateBoardCommand, Guid>
{
    public async Task<Guid> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found.");
        }

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsPublic = request.IsPublic
        };
        await unitOfWork.Repository<Board>().AddAsync(board);

        var userBoard = new UserBoard
        {
            UserId = request.UserId,
            BoardId = board.Id,
            BoardRole = BoardRole.Owner
        };

        await unitOfWork.Repository<UserBoard>().AddAsync(userBoard);
        await unitOfWork.SaveChangesAsync();

        return board.Id;
    }
}