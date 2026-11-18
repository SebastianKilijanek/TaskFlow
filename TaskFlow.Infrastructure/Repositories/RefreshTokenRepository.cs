using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class RefreshTokenRepository(TaskFlowDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByHashAsync(string hash, CancellationToken cancellationToken = default)
        => await context.RefreshTokens.FirstOrDefaultAsync(r => r.Id == hash, cancellationToken);
}