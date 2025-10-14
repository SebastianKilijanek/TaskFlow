using System.Text.Json.Serialization;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Base;

public abstract record CommentRequestBase(Guid UserId, Guid CommentId) 
    : IUserExistenceRequest, IEntityExistenceRequest<Comment>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Guid EntityId => CommentId;
    [JsonIgnore] public Comment Entity { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;

    protected virtual IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor];

    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((Entity.TaskItem.Column.BoardId, RequiredRoles));
    }
}