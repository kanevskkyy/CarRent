using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IReadOnlyDictionary<string, int>> GetDailyBookingCountsForLast48HoursAsync(CancellationToken cancellationToken = default);
}