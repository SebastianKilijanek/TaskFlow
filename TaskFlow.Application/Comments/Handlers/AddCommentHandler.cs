using MediatR;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Handlers;

public class AddCommentHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddCommentCommand, Guid>
{
    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskItemId,
            AuthorId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Repository<Comment>().AddAsync(comment);
        await unitOfWork.SaveChangesAsync();

        return comment.Id;
    }
}