using System.Text.Json.Serialization;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Base;

public abstract record TaskItemRequestBase(Guid UserId, Guid Id) : IUserExistenceRequest, IEntityExistenceRequest<TaskItem>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Guid EntityId => Id;
    [JsonIgnore] public TaskItem Entity { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;
    
    protected virtual IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor];

    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((Entity.Column.BoardId, RequiredRoles));
    }
}