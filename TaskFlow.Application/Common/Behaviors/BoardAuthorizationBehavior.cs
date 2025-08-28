using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class BoardAuthorizationBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IBoardAuthorizableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var (boardId, requiredRoles) = await request.GetAuthorizationDataAsync(unitOfWork);

        var board = await unitOfWork.Repository<Board>().GetByIdAsync(boardId);
        if (board is null)
            throw new NotFoundException($"Board with ID {boardId} not found.");

        request.Board = board;

        if (board.IsPublic)
            return await next();

        var userBoard = await unitOfWork.Repository<UserBoard>().GetByIdAsync(request.UserId, boardId);
        if (userBoard is null)
            throw new ForbiddenAccessException("You are not authorized to access this board.");

        if (requiredRoles != null && requiredRoles.Any() && !requiredRoles.Contains(userBoard.BoardRole))
            throw new ForbiddenAccessException("You do not have the required permissions for this action.");

        return await next();
    }
}