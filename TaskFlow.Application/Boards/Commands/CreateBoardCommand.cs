using MediatR;

namespace TaskFlow.Application.Boards.Commands;

public record CreateBoardCommand(Guid UserId, string Name, bool IsPublic) : IRequest<Guid>;