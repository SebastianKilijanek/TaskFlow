using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class ColumnExistenceCheckBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IColumnExistenceRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        request.Entity = await unitOfWork.Repository<Column>().GetByIdAsync(request.ColumnId, cancellationToken)
            ?? throw new NotFoundException($"Column with ID {request.ColumnId} not found.");

        return await next();
    }
}