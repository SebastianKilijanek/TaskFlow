using MediatR;
using TaskFlow.Application.Comments.Base;

namespace TaskFlow.Application.Comments.Commands;

public record DeleteCommentCommand(Guid UserId, Guid Id) : CommentRequestBase(UserId, Id), IRequest<Unit>;