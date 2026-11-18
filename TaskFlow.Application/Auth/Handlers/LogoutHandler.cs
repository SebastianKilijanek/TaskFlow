using MediatR;
using TaskFlow.Application.Auth.Commands;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Auth.Handlers;

public class LogoutHandler(IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var stored = await unitOfWork.RefreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            unitOfWork.RefreshTokenRepository.Update(stored);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}