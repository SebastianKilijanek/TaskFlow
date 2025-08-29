using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Repositories;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken: cancellationToken);
    }
        
    public virtual async Task<T?> GetByIdAsync(Guid id1, Guid id2, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id1, id2], cancellationToken: cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync(cancellationToken);
    }
        
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}