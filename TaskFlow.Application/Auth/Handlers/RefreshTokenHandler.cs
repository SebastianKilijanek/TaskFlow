using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Auth.Handlers;

public class RefreshTokenHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, AuthResultDTO>
{
    public async Task<AuthResultDTO?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromToken(request.RefreshToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");
            
        var email = principal.Identity?.Name;
        if (email == null)
            throw new UnauthorizedAccessException("Invalid token: email not found.");

        var user = await unitOfWork.UserRepository.GetByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var tokens = jwtService.GenerateTokens(user);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}