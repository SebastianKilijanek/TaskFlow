using MediatR;

namespace TaskFlow.Application.Columns.Commands;

public record MoveColumnCommand(Guid Id, int NewPosition) : IRequest<Unit>;