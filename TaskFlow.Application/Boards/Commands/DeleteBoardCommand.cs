using MediatR;

namespace TaskFlow.Application.Boards.Commands;

public record DeleteBoardCommand(Guid Id) : IRequest<Unit>;