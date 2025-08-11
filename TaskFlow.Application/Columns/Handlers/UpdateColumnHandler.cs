using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class UpdateColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateColumnCommand, Unit>
{
    public async Task<Unit> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(request.Id);
        if (column == null) throw new Exception("Column not found");

        column.Name = request.Name;
        column.Position = request.Position;

        unitOfWork.Repository<Column>().Update(column);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}