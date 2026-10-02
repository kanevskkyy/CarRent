namespace CarRent.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IBookingRepository Bookings { get; }
    ICarRepository Cars { get; }
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}