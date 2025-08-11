using TaskFlow.Application.Comments.DTO;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.TaskItems.DTO;

public class TaskItemDTO
{
    public TaskItemDTO() { }

    public TaskItemDTO(
        Guid id,
        string title,
        string? description,
        int position,
        Guid columnId,
        Guid? assignedUserId,
        string? assignedUserName,
        TaskItemStatus status,
        DateTime createdAt,
        List<CommentDTO> comments)
    {
        Id = id;
        Title = title;
        Description = description;
        Position = position;
        ColumnId = columnId;
        AssignedUserId = assignedUserId;
        AssignedUserName = assignedUserName;
        Status = status;
        CreatedAt = createdAt;
        Comments = comments;
    }

    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public required Guid ColumnId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentDTO> Comments { get; set; } = new();
}