namespace TaskFlow.Application.TaskItems.DTO;

public class MoveTaskItemDTO
{
    public required Guid NewColumnId { get; set; }
    public required int NewPosition { get; set; }
}