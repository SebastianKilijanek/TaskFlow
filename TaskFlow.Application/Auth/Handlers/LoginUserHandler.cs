using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Application.Auth.Handlers;

public class LoginUserHandler(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher<User> passwordHasher)
    : IRequestHandler<LoginUserCommand, AuthResultDTO?>
{
    public async Task<AuthResultDTO?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        var tokens = jwtService.GenerateTokens(user);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}