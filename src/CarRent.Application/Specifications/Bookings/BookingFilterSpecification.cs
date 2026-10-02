using Ardalis.Specification;
using CarRent.Application.Models.Bookings;
using CarRent.Domain.Entities;

namespace CarRent.Application.Specifications.Bookings;

public class BookingFilterSpecification : Specification<Booking>
{
    public BookingFilterSpecification(BookingFilterModel filter)
    {
        Query.AsNoTracking();
        
        if (filter.CarId.HasValue)
            Query.Where(b => b.CarId == filter.CarId.Value);

        if (filter.StartDate.HasValue)
            Query.Where(b => b.StartDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            Query.Where(b => b.EndDate <= filter.EndDate.Value);

        Query.Include(b => b.Car);
        Query.OrderBy(b => b.StartDate);
        
        Query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize);
    }
}