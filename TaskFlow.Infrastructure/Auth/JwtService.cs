using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Configuration;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Auth;

public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly string _jwtSecret = jwtOptions.Value.Secret ?? throw new ArgumentNullException("Jwt:Secret");
    private readonly string _jwtIssuer = jwtOptions.Value.Issuer ?? throw new ArgumentNullException("Jwt:Issuer");
    private readonly string _jwtAudience = jwtOptions.Value.Audience ?? throw new ArgumentNullException("Jwt:Audience");
    private readonly int _jwtExpiryMinutes = jwtOptions.Value.ExpiryMinutes ?? throw new ArgumentException("Jwt:ExpireMinutes");
    private readonly int _refreshExpiryMinutes = jwtOptions.Value.RefreshExpiryMinutes ?? throw new ArgumentException("Jwt:RefreshExpireMinutes");

    public (string AccessToken, string RefreshToken) GenerateTokens(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            signingCredentials: creds
        );

        var refreshToken = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_refreshExpiryMinutes),
            signingCredentials: creds
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            new JwtSecurityTokenHandler().WriteToken(refreshToken)
        );
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtIssuer,
            ValidAudience = _jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
            ValidateLifetime = validateLifetime
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwt
                || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}