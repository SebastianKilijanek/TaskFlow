using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public TaskItem() { }

    public TaskItem(Guid id, string title, string? description, int position, TaskItemStatus status, Guid columnId, Guid? assignedUserId, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Position = position;
        Status = status;
        ColumnId = columnId;
        AssignedUserId = assignedUserId;
        CreatedAt = createdAt;
    }

    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public TaskItemStatus Status { get; set; }
    public required Guid ColumnId { get; set; }
    public Column Column { get; set; } = null!;
    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public DateTime CreatedAt { get; set; }
}