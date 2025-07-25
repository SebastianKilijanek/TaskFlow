namespace TaskFlow.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }
    }
}