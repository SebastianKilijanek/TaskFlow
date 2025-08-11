using MediatR;
using TaskFlow.Application.Boards.DTO;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardsQuery : IRequest<IEnumerable<BoardDTO>>;