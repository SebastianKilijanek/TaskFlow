using MediatR;

namespace TaskFlow.Application.Boards.Commands
{
    public record UpdateBoardCommand(Guid Id, string Name, bool IsPublic) : IRequest<Unit>;
}