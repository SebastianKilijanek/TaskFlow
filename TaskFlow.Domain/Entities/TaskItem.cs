using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public TaskItemStatus Status { get; set; }
    public required Guid ColumnId { get; set; }
    public virtual Column Column { get; set; } = null!;
    public virtual Guid? AssignedUserId { get; set; }
    public virtual User? AssignedUser { get; set; }
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public DateTime CreatedAt { get; set; }
}