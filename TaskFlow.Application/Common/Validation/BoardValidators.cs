using FluentValidation;
using TaskFlow.Application.Boards.Commands;

namespace TaskFlow.Application.Common.Validation;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
    }
}

public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
    }
}