using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfRefreshTokenRepository : EfGenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public EfRefreshTokenRepository(CarRentDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresOnUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
