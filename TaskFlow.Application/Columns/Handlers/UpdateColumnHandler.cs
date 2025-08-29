using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class UpdateColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateColumnCommand, Unit>
{
    public async Task<Unit> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        request.Entity.Name = request.Name;
            
        unitOfWork.Repository<Column>().Update(request.Entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}