using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class TransferBoardOwnershipHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    : IRequestHandler<TransferBoardOwnershipCommand, Unit>
{
    public async Task<Unit> Handle(TransferBoardOwnershipCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.userIdToTransfer)
            throw new BadRequestException("You cannot transfer ownership to yourself.");

        var userBoardRepository = unitOfWork.Repository<UserBoard>();

        var currentOwnerUserBoard = await userBoardRepository.GetByIdAsync(request.UserId, request.BoardId);

        var newOwnerUserBoard = await userBoardRepository.GetByIdAsync(request.userIdToTransfer, request.BoardId);
        if (newOwnerUserBoard is null)
            throw new NotFoundException($"User with ID {request.userIdToTransfer} is not a member of this board.");

        currentOwnerUserBoard!.BoardRole = BoardRole.Editor;
        newOwnerUserBoard.BoardRole = BoardRole.Owner;

        userBoardRepository.Update(currentOwnerUserBoard);
        userBoardRepository.Update(newOwnerUserBoard);
        await unitOfWork.SaveChangesAsync();

        var newOwnerEmail = newOwnerUserBoard.User.Email;
        var boardName = newOwnerUserBoard.Board.Name;

        await emailService.SendAsync(
            newOwnerEmail,
            "TaskFlow: Board Ownership Transferred",
            $"You are now the owner of the board '{boardName}'."
        );

        return Unit.Value;
    }
}