using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class DeleteColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteColumnCommand, Unit>
{
    public async Task<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var columnRepository = unitOfWork.Repository<Column>();
        columnRepository.Remove(request.Entity);

        var remainingColumns = (await columnRepository
                .ListAsync(c => c.BoardId == request.Entity.BoardId && c.Id != request.Entity.Id))
                .OrderBy(c => c.Position)
                .ToList();

        // Reorder the position of the remaining columns
        for (int i = 0; i < remainingColumns.Count; i++)
        {
            var column = remainingColumns[i];
            if (column.Position != i)
            {
                column.Position = i;
                columnRepository.Update(column);
            }
        }

        await unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}