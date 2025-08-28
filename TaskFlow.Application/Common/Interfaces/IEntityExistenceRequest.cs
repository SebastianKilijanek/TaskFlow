namespace TaskFlow.Application.Common.Interfaces;

public interface IEntityExistenceRequest<TEntity> where TEntity : class
{
    Guid EntityId { get; }
    TEntity Entity { get; set; }
}