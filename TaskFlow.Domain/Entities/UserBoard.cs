using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class UserBoard
{
    public UserBoard() { }

    public UserBoard(Guid userId, Guid boardId, BoardRole boardRole)
    {
        UserId = userId;
        BoardId = boardId;
        BoardRole = boardRole;
        JoinedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid BoardId { get; set; }
    public virtual Board Board { get; set; } = null!;

    public BoardRole BoardRole { get; set; }
    public DateTime JoinedAt { get; set; }
}