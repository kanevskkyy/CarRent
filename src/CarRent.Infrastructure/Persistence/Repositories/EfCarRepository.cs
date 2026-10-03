using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Cars;
using CarRent.Application.Models.Pagination;
using CarRent.Application.Specifications.Cars;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfCarRepository : EfGenericRepository<Car>, ICarRepository
{
    public EfCarRepository(CarRentDbContext context) : base(context)
    {
    }

    public async Task<PagedList<Car>> GetPagedCarsWithFiltersAsync(CarFilterModel parameters, CancellationToken cancellationToken = default)
    {
        CarFilterSpecification specification = new CarFilterSpecification(parameters);
    
        IQueryable<Car> queryable = ApplySpecification(specification).AsNoTracking();

        int totalCount = await queryable.CountAsync(cancellationToken);

        List<Car> cars = await queryable
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Car>(cars, totalCount, parameters.PageNumber, parameters.PageSize);
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