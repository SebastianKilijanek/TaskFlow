using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Outbox;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class TransferBoardOwnershipHandler(IUnitOfWork unitOfWork, IOutbox outbox)
    : IRequestHandler<TransferBoardOwnershipCommand, Unit>
{
    public async Task<Unit> Handle(TransferBoardOwnershipCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.userIdToTransfer)
            throw new BadRequestException("You cannot transfer ownership to yourself.");

        var userBoardRepository = unitOfWork.Repository<UserBoard>();

        var currentOwnerUserBoard = await userBoardRepository.GetByIdAsync(request.UserId, request.BoardId, cancellationToken);

        var newOwnerUserBoard = await userBoardRepository.GetByIdAsync(request.userIdToTransfer, request.BoardId, cancellationToken);
        if (newOwnerUserBoard is null)
            throw new NotFoundException($"User with ID {request.userIdToTransfer} is not a member of this board.");

        currentOwnerUserBoard!.BoardRole = BoardRole.Editor;
        newOwnerUserBoard.BoardRole = BoardRole.Owner;

        userBoardRepository.Update(currentOwnerUserBoard);
        userBoardRepository.Update(newOwnerUserBoard);

        var newOwnerEmail = newOwnerUserBoard.User.Email;
        var boardName = newOwnerUserBoard.Board.Name;

        outbox.Add("email", new OutboxEmail(
            newOwnerEmail,
            "TaskFlow: Board Ownership Transferred",
            $"You are now the owner of the board '{boardName}'."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}