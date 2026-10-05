using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Payments;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;

namespace CarRent.Application.Features.Payments.Queries.GetPaymentStatus;

public class GetPaymentStatusQueryHandler : IQueryHandler<GetPaymentStatusQuery, PaymentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentStatusQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PaymentResponse> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        Booking? booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null || booking.UserId != request.UserId)
            throw new NotFoundException($"Booking with id {request.BookingId} not found.");

        Payment? payment = await _unitOfWork.Payments.GetByBookingIdAsync(booking.Id, cancellationToken);

        return new PaymentResponse
        {
            BookingId = booking.Id,
            BookingStatus = booking.Status,
            PaymentStatus = payment?.Status ?? PaymentStatus.Pending,
            Amount = payment?.Amount ?? booking.TotalPrice,
            PaidAt = payment?.PaidAt
        };
    }
}