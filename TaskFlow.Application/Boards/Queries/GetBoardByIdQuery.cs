using MediatR;
using TaskFlow.Application.Boards.DTO;

namespace TaskFlow.Application.Boards.Queries;

public record GetBoardByIdQuery(Guid Id) : IRequest<BoardDTO?>;