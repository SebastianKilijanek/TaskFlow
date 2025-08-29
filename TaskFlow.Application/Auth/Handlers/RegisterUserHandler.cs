using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Auth.DTO;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Auth.Handlers;

public class RegisterUserHandler(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher<User> passwordHasher)
    : IRequestHandler<RegisterUserCommand, AuthResultDTO>
{
    public async Task<AuthResultDTO> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("User with this email already exists.");
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = String.Empty,
            Role = UserRole.User
        };
        
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        await unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokens = jwtService.GenerateTokens(user);

        return new AuthResultDTO(tokens.AccessToken, tokens.RefreshToken, user.UserName, user.Email, user.Role.ToString());
    }
}