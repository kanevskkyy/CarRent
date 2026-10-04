using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Models.Bookings;
using CarRent.Application.Models.Pagination;

namespace CarRent.Application.Bookings.Queries.GetPagedBookings;

public record GetPagedBookingsQuery(
    BookingFilterModel Filter, 
    Guid UserId, 
    bool IsAdmin
) : IQuery<PagedList<BookingListResponse>>;