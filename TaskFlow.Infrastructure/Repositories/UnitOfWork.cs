using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories
{
    public class UnitOfWork(TaskFlowDbContext context) : IUnitOfWork, IDisposable
    {
        private readonly Dictionary<Type, object> _repositories = new();
       
        public IUserRepository UserRepository { get; } = new UserRepository(context);
        
        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.TryGetValue(typeof(T), out var repo))
                return (IRepository<T>)repo;

            var repository = new Repository<T>(context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}