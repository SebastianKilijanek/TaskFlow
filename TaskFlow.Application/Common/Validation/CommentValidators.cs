using FluentValidation;
using TaskFlow.Application.Comments.Commands;

namespace TaskFlow.Application.Common.Validation;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
    }
}