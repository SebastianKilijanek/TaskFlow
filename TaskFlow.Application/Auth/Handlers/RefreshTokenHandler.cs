using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Auth.Handlers;

public class RefreshTokenHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, AuthResultDTO>
{
    public async Task<AuthResultDTO> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromToken(request.RefreshToken, validateLifetime: false);
        if (principal is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var stored = await unitOfWork.RefreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var user = await unitOfWork.UserRepository.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        stored.RevokedAt = DateTime.UtcNow;
        unitOfWork.RefreshTokenRepository.Update(stored);

        var tokens = jwtService.GenerateTokens(user);
        var newToken = RefreshTokenFactory.Create(user.Id, tokens.RefreshToken, jwtService.RefreshTokenExpiresAt);
        stored.ReplacedByTokenId = newToken.Id;
        await unitOfWork.RefreshTokenRepository.AddAsync(newToken, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}