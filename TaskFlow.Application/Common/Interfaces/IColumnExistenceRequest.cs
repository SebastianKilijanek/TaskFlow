using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IColumnExistenceRequest
{
    Guid ColumnId { get; }
    Column Entity { get; set; }
}