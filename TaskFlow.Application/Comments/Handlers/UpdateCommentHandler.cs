using MediatR;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Handlers;

public class UpdateCommentHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateCommentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = request.Entity;

        if (comment.AuthorId != request.UserId)
            throw new ForbiddenAccessException("You are not authorized to update this comment.");

        comment.Content = request.Content;
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}