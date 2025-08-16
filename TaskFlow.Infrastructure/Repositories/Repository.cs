using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Repositories;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
        
    public virtual async Task<T?> GetByIdAsync(Guid id1, Guid id2)
    {
        return await _dbSet.FindAsync(id1, id2);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync();
    }
        
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
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