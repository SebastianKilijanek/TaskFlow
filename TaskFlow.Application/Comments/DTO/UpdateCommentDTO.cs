namespace TaskFlow.Application.Comments.DTO;

public class UpdateCommentDTO
{
    public required Guid CommentId { get; set; }
    public required string Content { get; set; }
}