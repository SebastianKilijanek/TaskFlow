using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface ICommentExistenceRequest
{
    Guid CommentId { get; }
    Comment Entity { get; set; }
}