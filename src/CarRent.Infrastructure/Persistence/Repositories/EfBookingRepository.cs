using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Bookings;
using CarRent.Application.Models.Pagination;
using CarRent.Application.Specifications.Bookings;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfBookingRepository : EfGenericRepository<Booking>, IBookingRepository
{
    public EfBookingRepository(CarRentDbContext context) : base(context)
    {
    }
    
    public async Task<IReadOnlyList<Car>> GetMostBookedCarsAsync(
        int count, CancellationToken cancellationToken = default)
    {
        BookingStatus[] countedStatuses = new[]
        {
            BookingStatus.Confirmed,
            BookingStatus.Completed
        };

        List<Guid> topCarIds = await dbSet
            .AsNoTracking()
            .Where(b => countedStatuses.Contains(b.Status))
            .GroupBy(b => b.CarId)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key) // stable order when counts are equal
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync(cancellationToken);

        if (topCarIds.Count == 0)
            return Array.Empty<Car>();

        List<Car> cars = await context.Set<Car>()
            .AsNoTracking()
            .Where(c => topCarIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        return topCarIds
            .Select(id => cars.FirstOrDefault(c => c.Id == id))
            .Where(c => c is not null)
            .Select(c => c!)
            .ToList();
    }
    
    public async Task<int> CancelExpiredPendingBookingsAsync(TimeSpan expirationTimeout, CancellationToken cancellationToken = default)
    {
        DateTime cutoffTime = DateTime.UtcNow.Subtract(expirationTimeout);
        
        List<Booking> expiredBookings = await dbSet
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt <= cutoffTime)
            .ToListAsync(cancellationToken);

        if (!expiredBookings.Any())
        {
            return 0;
        }

        foreach (Booking booking in expiredBookings)
        {
            booking.Status = BookingStatus.Cancelled;
        }

        await context.SaveChangesAsync(cancellationToken);
        return expiredBookings.Count;
    }
    
    public async Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        BookingStatus[] activeStatuses = new[]
        {
            BookingStatus.Pending, 
            BookingStatus.Confirmed
        };

        bool hasOverlap = await dbSet
            .AsNoTracking()
            .AnyAsync(b => b.CarId == carId 
                           && activeStatuses.Contains(b.Status)
                           && b.StartDate < endDate 
                           && b.EndDate > startDate, cancellationToken);

        return !hasOverlap;
    }

    public async Task<PagedList<Booking>> GetPagedBookingsWithFiltersAsync(BookingFilterModel parameters, CancellationToken cancellationToken = default)
    {
        BookingFilterSpecification specification = new BookingFilterSpecification(parameters);
    
        IQueryable<Booking> queryable = ApplySpecification(specification).AsNoTracking();

        int totalCount = await queryable.CountAsync(cancellationToken);

        List<Booking> bookings = await queryable
            .AsNoTracking()
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Booking>(bookings, totalCount, parameters.PageNumber, parameters.PageSize);
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
        
        List<DateTime> bookingDates = await dbSet
            .AsNoTracking()
            .Where(b => b.CreatedAt >= cutoffTime && validStatuses.Contains(b.Status))
            .Select(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        DayOfWeek[] daysOfWeekOrder = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        Dictionary<string, int> result = daysOfWeekOrder
            .ToDictionary(day => day.ToString(), _ => 0);

        Dictionary<string, int> actualCounts = bookingDates
            .GroupBy(b => b.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        
        foreach (var (day, count) in actualCounts)
        {
            result[day] = count;
        }

        return result;
    }
}