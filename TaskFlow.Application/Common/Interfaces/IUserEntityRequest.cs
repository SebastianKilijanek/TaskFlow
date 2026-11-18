using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IUserEntityRequest
{
    Guid UserId { get; }
    User Entity { get; set; }
}