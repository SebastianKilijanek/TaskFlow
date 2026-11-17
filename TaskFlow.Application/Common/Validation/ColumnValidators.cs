using FluentValidation;
using TaskFlow.Application.Columns.Commands;

namespace TaskFlow.Application.Common.Validation;

public class CreateColumnCommandValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
    }
}

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
    }
}

public class MoveColumnCommandValidator : AbstractValidator<MoveColumnCommand>
{
    public MoveColumnCommandValidator()
    {
        RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
    }
}