using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Payments;

namespace CarRent.Application.Features.Payments.Queries.GetPaymentStatus;

public record GetPaymentStatusQuery(Guid BookingId, Guid UserId) : IQuery<PaymentResponse>;