using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Payments;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Payments;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;

namespace CarRent.Application.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public CreatePaymentCommandHandler(IUnitOfWork unitOfWork, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await _unitOfWork.Bookings.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null || booking.UserId != command.UserId)
            throw new NotFoundException($"Booking with id {command.BookingId} not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new ValidationException("Only pending bookings can be paid (it may have expired or been cancelled).");

        Payment? payment = await _unitOfWork.Payments.GetByBookingIdAsync(booking.Id, cancellationToken);
        if (payment is { Status: PaymentStatus.Succeeded or PaymentStatus.Refunded })
            throw new ConflictException("This booking is already paid.");

        PaymentOrder order = await _paymentGateway.CreateOrderAsync(
            booking.TotalPrice,
            $"Car rental booking {booking.Id}",
            booking.Id.ToString(),
            cancellationToken);

        if (payment is null)
        {
            payment = new Payment
            {
                BookingId = booking.Id,
                TransactionId = order.OrderId,
                Amount = booking.TotalPrice,
                Status = PaymentStatus.Pending
            };
            await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        }
        else
        {
            payment.TransactionId = order.OrderId;
            payment.Amount = booking.TotalPrice;
            payment.Status = PaymentStatus.Pending;
            _unitOfWork.Payments.Update(payment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePaymentResponse
        {
            BookingId = booking.Id,
            OrderId = order.OrderId,
            ApprovalUrl = order.ApprovalUrl
        };
    }
}