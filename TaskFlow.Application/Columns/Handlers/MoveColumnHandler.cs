using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class MoveColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<MoveColumnCommand, Unit>
{
    public async Task<Unit> Handle(MoveColumnCommand request, CancellationToken cancellationToken)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(request.Id);
        if (column == null) throw new Exception("Column not found");

        column.Position = request.NewPosition;

        unitOfWork.Repository<Column>().Update(column);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}