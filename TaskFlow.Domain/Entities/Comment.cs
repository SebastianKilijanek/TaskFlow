namespace TaskFlow.Domain.Entities
{
    public class Comment
    {
        public required Guid Id { get; set; }
        public required string Content { get; set; }
        public required Guid TaskItemId { get; set; }
        public virtual TaskItem TaskItem { get; set; } = null!;
        public required Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}