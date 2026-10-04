using CarRent.Application.Abstractions;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using MediatR;

namespace CarRent.Application.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public CancelBookingCommandHandler(IUnitOfWork unitOfWork, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<Unit> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await _unitOfWork.Bookings.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
            throw new NotFoundException($"Booking with id {command.BookingId} not found.");

        if (!command.IsAdmin && booking.UserId != command.UserId)
            throw new ValidationException("You do not have permission to cancel this booking.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new ValidationException("Booking is already cancelled.");

        Payment? payment = await _unitOfWork.Payments.GetByBookingIdAsync(booking.Id, cancellationToken);
        if (payment is { Status: PaymentStatus.Succeeded })
        {
            await _paymentGateway.RefundOrderAsync(payment.TransactionId, cancellationToken);

            payment.Status = PaymentStatus.Refunded;
            _unitOfWork.Payments.Update(payment);
        }

        booking.Status = BookingStatus.Cancelled;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}