using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common;

public static class RefreshTokenFactory
{
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
        => new()
        {
            Id = RefreshTokenHasher.Hash(token),
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
        };
}