using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
}