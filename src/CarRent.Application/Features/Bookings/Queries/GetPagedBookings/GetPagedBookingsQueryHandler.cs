using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Bookings.Queries.GetPagedBookings;

public class GetPagedBookingsQueryHandler : IQueryHandler<GetPagedBookingsQuery, PagedList<BookingListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPagedBookingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedList<BookingListResponse>> Handle(GetPagedBookingsQuery request, CancellationToken cancellationToken)
    { 
        if (!request.IsAdmin)
            request.Filter.UserId = request.UserId;

        PagedList<Booking> pagedBookings = await _unitOfWork.Bookings.GetPagedBookingsWithFiltersAsync(
            request.Filter, 
            cancellationToken);

        List<BookingListResponse> mappedItems = _mapper.Map<List<BookingListResponse>>(pagedBookings.Items);

        return new PagedList<BookingListResponse>(
            mappedItems,
            pagedBookings.TotalCount,
            pagedBookings.CurrentPage,
            pagedBookings.PageSize);
    }
}