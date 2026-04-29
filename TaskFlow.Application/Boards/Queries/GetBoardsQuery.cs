using MediatR;
using TaskFlow.Application.Boards.DTO;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<IReadOnlyList<BoardDTO>>;