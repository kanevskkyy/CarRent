using CarRent.Domain.Enums;

namespace CarRent.Application.DTOs.Payments;

public class PaymentResponse
{
    public Guid BookingId { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}