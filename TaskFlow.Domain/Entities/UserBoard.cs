using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class UserBoard
{
    public required Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public required Guid BoardId { get; set; }
    public virtual Board Board { get; set; } = null!;

    public required BoardRole BoardRole { get; set; }
    public DateTime JoinedAt { get; set; }
}