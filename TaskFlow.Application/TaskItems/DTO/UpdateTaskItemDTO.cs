namespace TaskFlow.Application.TaskItems.DTO;

public class UpdateTaskItemDTO
{
    public required string Title { get; set; }
    public string Description { get; set; }
    public required int Status { get; set; }
    public Guid? AssignedUserId { get; set; }
}