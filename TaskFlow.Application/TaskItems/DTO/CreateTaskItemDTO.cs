namespace TaskFlow.Application.TaskItems.DTO;

public class CreateTaskItemDTO
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid ColumnId { get; set; }
}