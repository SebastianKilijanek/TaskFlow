namespace TaskFlow.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
}