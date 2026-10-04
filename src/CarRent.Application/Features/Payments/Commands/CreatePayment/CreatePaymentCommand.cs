using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Payments;

namespace CarRent.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(Guid BookingId, Guid UserId) : ICommand<CreatePaymentResponse>;