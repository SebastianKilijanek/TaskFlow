using MediatR;
using TaskFlow.Application.Columns.Commands;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Columns.Handlers;

public class MoveColumnHandler(IUnitOfWork unitOfWork) : IRequestHandler<MoveColumnCommand, Unit>
{
    public async Task<Unit> Handle(MoveColumnCommand request, CancellationToken cancellationToken)
    {
        var columnRepository = unitOfWork.Repository<Column>();
        
        var columnsOnBoard = (await columnRepository
                .ListAsync(c => c.BoardId == request.Column.BoardId))
                .OrderBy(c => c.Position)
                .ToList();

        var columnInList = columnsOnBoard.FirstOrDefault(c => c.Id == request.Column.Id);
        if (columnInList is null)
        {
            throw new ConflictException("Column is not part of the specified board.");
        }

        columnsOnBoard.Remove(columnInList);

        var newPosition = Math.Clamp(request.NewPosition, 0, columnsOnBoard.Count);

        columnsOnBoard.Insert(newPosition, columnInList);

        // Reorder the position of all columns on the board
        for (var i = 0; i < columnsOnBoard.Count; i++)
        {
            var column = columnsOnBoard[i];
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