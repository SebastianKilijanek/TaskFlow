using FluentValidation;
using TaskFlow.Application.UserBoards.Commands;

namespace TaskFlow.Application.Common.Validation;

public class AddUserToBoardCommandValidator : AbstractValidator<AddUserToBoardCommand>
{
    public AddUserToBoardCommandValidator()
    {
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
    }
}

public class TransferBoardOwnershipCommandValidator : AbstractValidator<TransferBoardOwnershipCommand>
{
    public TransferBoardOwnershipCommandValidator()
    {
        RuleFor(x => x.userIdToTransfer).NotEqual(x => x.UserId)
            .WithMessage("Cannot transfer ownership to yourself.");
    }
}