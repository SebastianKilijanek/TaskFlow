using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IUserExistenceRequest
{
    Guid UserId { get; }
    User User { get; set; }
}