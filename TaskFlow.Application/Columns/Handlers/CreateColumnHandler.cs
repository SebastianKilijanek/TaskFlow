using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class CreateColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateColumnCommand, Guid>
{
    public async Task<Guid> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        var column = new Column
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BoardId = request.BoardId,
            Position = request.Position
        };
        
        await unitOfWork.Repository<Column>().AddAsync(column);
        await unitOfWork.SaveChangesAsync();
        
        return column.Id;
    }
}