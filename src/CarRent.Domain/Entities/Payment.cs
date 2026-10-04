using CarRent.Domain.Enums;

namespace CarRent.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } =  Guid.NewGuid();

    public Guid BookingId { get; set; } 
    public Booking Booking { get; set; } = null!;

    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime? PaidAt { get; set; } 
}