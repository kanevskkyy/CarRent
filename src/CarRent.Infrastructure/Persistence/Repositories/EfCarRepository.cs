using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class CarRepository : GenericRepository<Car>, ICarRepository
{
    public CarRepository(CarRentDbContext context) : base(context)
    {
    }

    public async Task<Car?> GetCarByIdWithBookingsAsync(Guid carId, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        return await dbSet
            .AsNoTracking()
            .Include(c => c.Bookings
                .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                            && b.EndDate >= now))
            .FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);
    }
}