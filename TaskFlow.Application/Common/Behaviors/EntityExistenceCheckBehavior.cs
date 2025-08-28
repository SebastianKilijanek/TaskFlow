using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class EntityExistenceCheckBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IEntityExistenceRequest<object>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var entityType = typeof(TRequest).GetInterface(typeof(IEntityExistenceRequest<>).Name)!
            .GetGenericArguments()[0];
        
        var repository = unitOfWork.GetType()
            .GetMethod(nameof(IUnitOfWork.Repository))!
            .MakeGenericMethod(entityType)
            .Invoke(unitOfWork, null);

        var getByIdAsync = repository!.GetType().GetMethod("GetByIdAsync", [typeof(Guid)]);

        var entity = await (dynamic)getByIdAsync!.Invoke(repository, [request.EntityId])!;

        if (entity is null)
        {
            throw new NotFoundException($"{entityType.Name} with ID {request.EntityId} not found.");
        }

        request.Entity = entity;

        return await next();
    }
}