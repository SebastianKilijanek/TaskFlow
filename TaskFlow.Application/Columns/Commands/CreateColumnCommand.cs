using MediatR;

namespace TaskFlow.Application.Columns.Commands;

public record CreateColumnCommand(string Name, Guid BoardId, int Position) : IRequest<Guid>;