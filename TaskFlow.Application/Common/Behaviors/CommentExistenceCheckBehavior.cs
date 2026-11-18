using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class CommentExistenceCheckBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommentExistenceRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        request.Entity = await unitOfWork.Repository<Comment>().GetByIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment with ID {request.CommentId} not found.");

        return await next();
    }
}