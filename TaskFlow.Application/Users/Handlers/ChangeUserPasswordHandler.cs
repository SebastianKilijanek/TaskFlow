using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Users.Handlers;

public class ChangeUserPasswordHandler(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher) 
    : IRequestHandler<ChangeUserPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var userRepository = unitOfWork.Repository<User>();

        var verificationResult = passwordHasher.VerifyHashedPassword(request.User, request.User.PasswordHash, request.CurrentPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("Current password is incorrect.");
        }

        request.User.PasswordHash = passwordHasher.HashPassword(request.User, request.NewPassword);
        userRepository.Update(request.User);

        var refreshTokens = await unitOfWork.RefreshTokenRepository
            .ListAsync(r => r.UserId == request.UserId, cancellationToken);
        foreach (var token in refreshTokens)
        {
            if (token.RevokedAt is null)
            {
                token.RevokedAt = DateTime.UtcNow;
                unitOfWork.RefreshTokenRepository.Update(token);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}