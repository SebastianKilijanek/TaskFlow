namespace TaskFlow.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> ListAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}