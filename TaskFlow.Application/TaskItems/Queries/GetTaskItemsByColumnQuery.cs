using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.TaskItems.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.TaskItems.Queries;

public record GetTaskItemsByColumnQuery(Guid UserId, Guid ColumnId) 
    : IRequest<IReadOnlyList<TaskItemDTO>>, IUserExistenceRequest, IEntityExistenceRequest<Column>, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Guid EntityId => ColumnId;
    [JsonIgnore] public Column Entity { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;
    
    public async Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return await Task.FromResult((Board.Id, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer]));
    }
}