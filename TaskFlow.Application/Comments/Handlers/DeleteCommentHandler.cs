using MediatR;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Handlers;

public class DeleteCommentHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCommentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = request.Entity;

        if (comment.AuthorId != request.UserId)
            throw new ForbiddenAccessException("You are not authorized to delete this comment.");

        unitOfWork.Repository<Comment>().Remove(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}