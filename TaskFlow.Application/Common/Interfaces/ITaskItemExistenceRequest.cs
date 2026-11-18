using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface ITaskItemExistenceRequest
{
    Guid TaskItemId { get; }
    TaskItem Entity { get; set; }
}