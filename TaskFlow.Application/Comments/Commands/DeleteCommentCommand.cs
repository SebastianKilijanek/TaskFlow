using MediatR;
using TaskFlow.Application.Comments.Base;

namespace TaskFlow.Application.Comments.Commands;

public record DeleteCommentCommand(Guid UserId, Guid CommentId) : CommentRequestBase(UserId, CommentId), IRequest<Unit>;