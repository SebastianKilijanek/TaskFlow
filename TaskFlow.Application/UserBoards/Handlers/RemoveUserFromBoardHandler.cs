using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class RemoveUserFromBoardHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    : IRequestHandler<RemoveUserFromBoardCommand, Unit>
{
    public async Task<Unit> Handle(RemoveUserFromBoardCommand request, CancellationToken cancellationToken)
    {
        if (request.UserIdToRemove == request.UserId)
            throw new BadRequestException("An owner cannot remove themselves from the board.");

        var userBoardToRemove = await unitOfWork.Repository<UserBoard>()
            .GetByIdAsync(request.UserIdToRemove, request.BoardId, cancellationToken);

        if (userBoardToRemove is null)
            throw new NotFoundException($"User with ID {request.UserIdToRemove} not found on board with ID {request.BoardId}.");

        var userEmail = userBoardToRemove.User.Email;
        var boardName = userBoardToRemove.Board.Name;

        unitOfWork.Repository<UserBoard>().Remove(userBoardToRemove);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailService.SendAsync(
            userEmail,
            "TaskFlow: Removed from Board",
            $"You have been removed from the board '{boardName}'."
        );

        return Unit.Value;
    }
}