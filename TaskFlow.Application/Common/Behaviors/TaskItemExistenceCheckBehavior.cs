using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class TaskItemExistenceCheckBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITaskItemExistenceRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        request.Entity = await unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskItemId, cancellationToken)
            ?? throw new NotFoundException($"TaskItem with ID {request.TaskItemId} not found.");

        return await next();
    }
}