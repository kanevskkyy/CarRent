using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfPaymentRepository : EfGenericRepository<Payment>, IPaymentRepository
{
    public EfPaymentRepository(CarRentDbContext context) : base(context)
    {
        
    }

    public Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default) =>
        dbSet.Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

    public Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        dbSet.FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
}