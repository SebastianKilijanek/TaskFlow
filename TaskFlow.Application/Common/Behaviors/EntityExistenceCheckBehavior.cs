using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class EntityExistenceCheckBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var interfaceType = request.GetType()
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityExistenceRequest<>));

        if (interfaceType == null)
        {
            return await next();
        }

        var entityType = interfaceType.GetGenericArguments()[0];
        
        var entityIdProperty = interfaceType.GetProperty(nameof(IEntityExistenceRequest<object>.EntityId));
        var entityId = (Guid)entityIdProperty!.GetValue(request)!;

        var repository = unitOfWork.GetType()
            .GetMethod(nameof(IUnitOfWork.Repository))!
            .MakeGenericMethod(entityType)
            .Invoke(unitOfWork, null);

        var getByIdAsync = repository!.GetType().GetMethod("GetByIdAsync", [typeof(Guid), typeof(CancellationToken)]);

        var entity = await (dynamic)getByIdAsync!.Invoke(repository, [entityId, cancellationToken])!;

        if (entity is null)
        {
            throw new NotFoundException($"{entityType.Name} with ID {entityId} not found.");
        }

        var entityProperty = interfaceType.GetProperty(nameof(IEntityExistenceRequest<object>.Entity));
        entityProperty!.SetValue(request, entity);

        return await next();
    }
}