using System.Text.Json.Serialization;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Base;

public abstract record ColumnRequestBase(Guid UserId, Guid Id) : IUserExistenceRequest, IEntityExistenceRequest<Column>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Guid EntityId => Id;
    [JsonIgnore] public Column Entity { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;
    
    protected virtual IEnumerable<BoardRole> RequiredRoles => [BoardRole.Owner, BoardRole.Editor];

    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return (Entity.BoardId, RequiredRoles);
    }
}