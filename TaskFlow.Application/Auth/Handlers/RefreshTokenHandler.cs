using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Auth.Handlers;

public class RefreshTokenHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, AuthResultDTO?>
{
    public async Task<AuthResultDTO?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromToken(request.RefreshToken);
        if (principal == null)
            return null;

        var email = principal.Identity?.Name;
        if (email == null)
            return null;

        var user = await unitOfWork.UserRepository.GetByEmailAsync(email);
        if (user == null)
            return null;

        var tokens = jwtService.GenerateTokens(user);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}