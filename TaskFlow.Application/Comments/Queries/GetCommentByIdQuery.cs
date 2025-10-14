using MediatR;
using TaskFlow.Application.Comments.Base;
using TaskFlow.Application.Comments.DTO;

namespace TaskFlow.Application.Comments.Queries;

public record GetCommentByIdQuery(Guid UserId, Guid CommentId) : CommentRequestBase(UserId, CommentId), IRequest<CommentDTO>;