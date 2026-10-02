using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Repositories;

public interface ICarRepository : IGenericRepository<Car>
{
    Task<Car> GetCarByIdWithBookingsAsync(Guid carId, CancellationToken cancellationToken = default);
}