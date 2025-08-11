using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class DeleteColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteColumnCommand, Unit>
{
    public async Task<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var column = await unitOfWork.Repository<Column>().GetByIdAsync(request.Id);
        if (column == null) throw new Exception("Column not found");

        unitOfWork.Repository<Column>().Remove(column);
        await unitOfWork.SaveChangesAsync();
        
        return Unit.Value;
    }
}