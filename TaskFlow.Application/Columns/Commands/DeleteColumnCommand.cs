using MediatR;

namespace TaskFlow.Application.Columns.Commands;

public record DeleteColumnCommand(Guid Id) : IRequest<Unit>;