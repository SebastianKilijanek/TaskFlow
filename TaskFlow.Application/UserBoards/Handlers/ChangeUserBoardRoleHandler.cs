using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.UserBoards.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.UserBoards.Handlers;

public class ChangeUserBoardRoleHandler(IUnitOfWork unitOfWork, IEmailService emailService) 
    : IRequestHandler<ChangeUserBoardRoleCommand, Unit>
{
    public async Task<Unit> Handle(ChangeUserBoardRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.UserIdToChange)
            throw new BadRequestException("An owner cannot change their own role. Please transfer ownership instead.");

        if (request.NewRole == BoardRole.Owner)
            throw new BadRequestException("Cannot assign the Owner role. Please use the transfer ownership feature.");

        var userBoardToChange = await unitOfWork.Repository<UserBoard>()
            .GetByIdAsync(request.UserIdToChange, request.BoardId, cancellationToken);

        if (userBoardToChange is null)
            throw new NotFoundException($"User with ID {request.UserIdToChange} is not a member of this board.");

        userBoardToChange.BoardRole = request.NewRole;
        unitOfWork.Repository<UserBoard>().Update(userBoardToChange);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userEmail = userBoardToChange.User.Email;
        var boardName = userBoardToChange.Board.Name;

        await emailService.SendAsync(
            userEmail,
            "TaskFlow: Your Role Has Changed",
            $"Your role on the board '{boardName}' has been changed to {request.NewRole}."
        );

        return Unit.Value;
    }
}