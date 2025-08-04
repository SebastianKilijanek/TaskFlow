namespace TaskFlow.Application.Configuration;

public class JwtOptions
{
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int? ExpiryMinutes { get; set; }
    public required int? RefreshExpiryMinutes { get; set; }
}