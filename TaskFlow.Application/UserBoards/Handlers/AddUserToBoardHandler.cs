using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class AddUserToBoardHandler(IUnitOfWork unitOfWork, IEmailService emailService) : IRequestHandler<AddUserToBoardCommand, Unit>
{
    public async Task<Unit> Handle(AddUserToBoardCommand request, CancellationToken cancellationToken)
    {
        var userBoardRepo = unitOfWork.Repository<UserBoard>();

        var requesterUserBoard = await userBoardRepo.GetByIdAsync(request.UserId, request.BoardId);
        if (requesterUserBoard is null || requesterUserBoard.BoardRole != BoardRole.Owner)
        {
            throw new ForbiddenAccessException("Only the board owner can add users.");
        }
        
        var userToAdd = await unitOfWork.UserRepository.GetByEmailAsync(request.UserEmail);
        if (userToAdd is null)
        {
            throw new NotFoundException($"User with email '{request.UserEmail}' not found.");
        }

        var board = await unitOfWork.Repository<Board>().GetByIdAsync(request.BoardId);

        if (board!.UserBoards.Any(ub => ub.UserId == userToAdd.Id))
        {
            throw new ConflictException($"User '{request.UserEmail}' is already a member of this board.");
        }

        if (request.BoardRole == BoardRole.Owner)
        {
            throw new BadRequestException("A board can only have one owner. Please transfer ownership instead.");
        }

        var newUserBoard = new UserBoard(userToAdd.Id, request.BoardId, request.BoardRole);

        await unitOfWork.Repository<UserBoard>().AddAsync(newUserBoard);
        await unitOfWork.SaveChangesAsync();

        await emailService.SendAsync(
            userToAdd.Email,
            "TaskFlow: You have been added to a board",
            $"You have been added to the board '{board.Name}' as a {request.BoardRole}."
        );

        return Unit.Value;
    }
}