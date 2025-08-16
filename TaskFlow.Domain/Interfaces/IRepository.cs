using System.Linq.Expressions;

namespace TaskFlow.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByIdAsync(Guid id1, Guid id2);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}