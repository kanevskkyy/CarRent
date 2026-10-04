using CarRent.Application.Interfaces.Repositories;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IBookingRepository Bookings { get; }
    public ICarRepository Cars { get; }
    public IPaymentRepository Payments { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    
    private CarRentDbContext context;

    public UnitOfWork(
        CarRentDbContext context,
        IBookingRepository bookings,
        ICarRepository cars,
        IPaymentRepository payments,
        IRefreshTokenRepository refreshTokens)
    {
        this.context = context;
        this.Bookings = bookings;
        this.Cars = cars;
        this.Payments = payments;
        this.RefreshTokens = refreshTokens;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}