namespace TaskFlow.Application.Comments.DTO;

public class AddCommentDTO
{
    public required Guid TaskItemId { get; set; }
    public required string Content { get; set; }
}