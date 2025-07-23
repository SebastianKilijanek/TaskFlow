using MediatR;

namespace TaskFlow.Application.Boards.Commands
{
    public record CreateBoardCommand(string Name, bool IsPublic) : IRequest<Guid>;
}