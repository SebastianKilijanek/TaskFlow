using FluentValidation;
using TaskFlow.Application.TaskItems.Commands;

namespace TaskFlow.Application.Common.Validation;

public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
{
    public CreateTaskItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(1, 200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
{
    public UpdateTaskItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(1, 200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).InclusiveBetween(0, 3);
    }
}

public class MoveTaskItemCommandValidator : AbstractValidator<MoveTaskItemCommand>
{
    public MoveTaskItemCommandValidator()
    {
        RuleFor(x => x.NewPosition).GreaterThanOrEqualTo(0);
    }
}