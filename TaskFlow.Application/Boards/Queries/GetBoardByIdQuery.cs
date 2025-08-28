using System.Text.Json.Serialization;
using MediatR;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardByIdQuery(Guid UserId, Guid Id) : IRequest<BoardDTO>, IUserExistenceRequest, IBoardAuthorizableRequest
{
    [JsonIgnore] public User User { get; set; } = null!;
    [JsonIgnore] public Board Board { get; set; } = null!;

    public Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork)
    {
        return Task.FromResult((Id, (IEnumerable<BoardRole>)[BoardRole.Owner, BoardRole.Editor, BoardRole.Viewer]));
    }
}