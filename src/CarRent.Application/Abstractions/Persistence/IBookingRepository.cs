using CarRent.Application.Models.Bookings;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<PagedList<Booking>> GetPagedBookingsWithFiltersAsync(BookingFilterModel parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, int>> GetDailyBookingCountsForLast48HoursAsync(CancellationToken cancellationToken = default);
    Task<int> CancelExpiredPendingBookingsAsync(TimeSpan expirationTimeout, CancellationToken cancellationToken = default);
}