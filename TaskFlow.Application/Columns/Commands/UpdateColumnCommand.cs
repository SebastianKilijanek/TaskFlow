using MediatR;

namespace TaskFlow.Application.Columns.Commands;

public record UpdateColumnCommand(Guid Id, string Name, int Position) : IRequest<Unit>;