using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class UpdateColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateColumnCommand, Unit>
{
    public async Task<Unit> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        request.Column.Name = request.Name;
            
        unitOfWork.Repository<Column>().Update(request.Column);
        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}