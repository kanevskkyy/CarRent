using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;

namespace CarRent.Application.Services;

public class PaymentConfirmationService : IPaymentConfirmationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentConfirmationService(IUnitOfWork unitOfWork, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task ConfirmAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (payment.Status == PaymentStatus.Succeeded)
            return;

        Booking booking = payment.Booking;

        if (booking.Status != BookingStatus.Pending)
            throw new ValidationException(
                "Booking is no longer pending (it may have expired). The payment was not taken.");

        await _paymentGateway.CaptureOrderAsync(payment.TransactionId, cancellationToken);

        payment.Status = PaymentStatus.Succeeded;
        payment.PaidAt = DateTime.UtcNow;
        booking.Status = BookingStatus.Confirmed;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}