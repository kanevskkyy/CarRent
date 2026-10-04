using Ardalis.Specification;
using CarRent.Application.Models.Cars;
using CarRent.Domain.Entities;

namespace CarRent.Application.Specifications.Cars;

public class CarFilterSpecification : Specification<Car>
{
    public CarFilterSpecification(CarFilterModel filter)
    {
        if (filter.BodyType.HasValue)
            Query.Where(c => c.BodyType == filter.BodyType.Value);

        if (filter.TransmissionType.HasValue)
            Query.Where(c => c.TransmissionType == filter.TransmissionType.Value);

        if (filter.MinPrice.HasValue)
            Query.Where(c => c.DailyPrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            Query.Where(c => c.DailyPrice <= filter.MaxPrice.Value);

        Query.OrderBy(c => c.DailyPrice);
    }
}