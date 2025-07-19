namespace TaskFlow.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ColumnId { get; set; }
        public Column Column { get; set; }
        public Guid? AssignedUserId { get; set; }
        public User AssignedUser { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int Position { get; set; }
    }
}