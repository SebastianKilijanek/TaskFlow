using MediatR;
using TaskFlow.Application.Comments.Base;

namespace TaskFlow.Application.Comments.Commands;

public record UpdateCommentCommand(Guid UserId, Guid CommentId, string Content) : CommentRequestBase(UserId, CommentId), IRequest<Unit>;