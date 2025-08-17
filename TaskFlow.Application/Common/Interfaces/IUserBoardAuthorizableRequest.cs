using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Interfaces;

public interface IUserBoardAuthorizableRequest
{
    Guid UserId { get; }
    Task<(Guid BoardId, IEnumerable<BoardRole> RequiredRoles)> GetAuthorizationDataAsync(IUnitOfWork unitOfWork);
}