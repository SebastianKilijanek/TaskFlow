using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}