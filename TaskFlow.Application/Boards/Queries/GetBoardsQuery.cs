using MediatR;
using TaskFlow.Application.Boards.DTO;
using TaskFlow.Application.Common;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<BoardDTO>>;