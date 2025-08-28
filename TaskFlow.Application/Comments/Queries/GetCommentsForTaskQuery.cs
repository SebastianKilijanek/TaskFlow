using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Comments.DTO;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Comments.Queries;

public record GetCommentsForTaskQuery(Guid UserId, Guid TaskItemId)
    : IRequest<IReadOnlyList<CommentDTO>>, IUserExistenceRequest, IEntityExistenceRequest<TaskItem>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Guid EntityId => TaskItemId;
    [JsonIgnore] public TaskItem Entity { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;

    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((Entity.Column.BoardId, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer]));
    }
}