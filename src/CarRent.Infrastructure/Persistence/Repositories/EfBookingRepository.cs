using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(CarRentDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyDictionary<string, int>> GetDailyBookingCountsForLast48HoursAsync(
        CancellationToken cancellationToken = default)
    {
        DateTime cutoffTime = DateTime.UtcNow.AddHours(-48);

        BookingStatus[] validStatuses = new[] 
        { 
            BookingStatus.Pending, 
            BookingStatus.Confirmed, 
            BookingStatus.Completed 
        };

        List<DateTime> bookings = await dbSet
            .AsNoTracking()
            .Where(b => b.CreatedAt >= cutoffTime && validStatuses.Contains(b.Status))
            .Select(b => b.CreatedAt)
            .ToListAsync(cancellationToken);


        Dictionary<string, int> result = bookings
            .GroupBy(b => b.DayOfWeek.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );
        return result;
    }
}