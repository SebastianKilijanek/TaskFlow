using System.Security.Claims;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IJwtService
{
    DateTime RefreshTokenExpiresAt { get; }
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true);
}