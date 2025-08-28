using MediatR;
using TaskFlow.Application.Comments.Base;

namespace TaskFlow.Application.Comments.Commands;

public record UpdateCommentCommand(Guid UserId, Guid Id, string Content) : CommentRequestBase(UserId, Id), IRequest<Unit>;