namespace CarRent.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IBookingRepository Bookings { get; }
    ICarRepository Cars { get; }
    IPaymentRepository Payments { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}