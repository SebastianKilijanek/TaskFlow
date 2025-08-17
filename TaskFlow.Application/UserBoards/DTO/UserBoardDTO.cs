namespace TaskFlow.Application.UserBoards.DTO;

public class UserBoardDTO
{
    public UserBoardDTO() { }
    
    public UserBoardDTO(Guid userId, Guid boardId, string boardRole, DateTime joinedAt)
    {
        UserId = userId;
        BoardId = boardId;
        BoardRole = boardRole;
        JoinedAt = joinedAt;
    }
    
    public required Guid UserId { get; set; }
    public required Guid BoardId { get; set; }
    public required string BoardRole { get; set; }
    public required DateTime JoinedAt { get; set; }
}