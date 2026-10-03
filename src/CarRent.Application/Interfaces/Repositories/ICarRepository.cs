using CarRent.Application.Models.Cars;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Repositories;

public interface ICarRepository : IGenericRepository<Car>
{
    Task<PagedList<Car>> GetPagedCarsWithFiltersAsync(CarFilterModel parameters, CancellationToken cancellationToken = default);
    Task<Car> GetCarByIdWithBookingsAsync(Guid carId, CancellationToken cancellationToken = default);
}