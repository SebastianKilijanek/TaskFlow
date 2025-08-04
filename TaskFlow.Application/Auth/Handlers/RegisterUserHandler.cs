using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Auth.Handlers;

public class RegisterUserHandler(
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    IPasswordHasher<User> passwordHasher)
    : IRequestHandler<RegisterUserCommand, AuthResultDTO>
{
    public async Task<AuthResultDTO> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.UserName,
            Role = UserRole.User
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        await unitOfWork.Repository<User>().AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        var tokens = jwtService.GenerateTokens(user);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}