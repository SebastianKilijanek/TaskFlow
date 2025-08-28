namespace TaskFlow.Application.Comments.DTO
{
    public class CommentDTO
    {
        public CommentDTO() { }

        public CommentDTO(Guid id, string content, Guid taskItemId, Guid authorId, string authorUserName, DateTime createdAt)
        {
            Id = id;
            Content = content;
            TaskItemId = taskItemId;
            AuthorId = authorId;
            AuthorUserName = authorUserName;
            CreatedAt = createdAt;
        }

        public required Guid Id { get; set; }
        public required string Content { get; set; }
        public required Guid TaskItemId { get; set; }
        public required Guid AuthorId { get; set; }
        public required string AuthorUserName { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}