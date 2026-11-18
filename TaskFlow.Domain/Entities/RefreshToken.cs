namespace TaskFlow.Domain.Entities;

public class RefreshToken
{
    public string Id { get; set; } = null!; // SHA-256 hash of the token
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenId { get; set; }
}