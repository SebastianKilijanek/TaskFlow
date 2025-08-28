using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Interfaces;

public interface IBoardAuthorizableRequest
{
    Guid UserId { get; }
    Board Board { get; set; }
    Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork);
}