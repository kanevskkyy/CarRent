using CarRent.Domain.Enums;

namespace CarRent.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid CarId { get; set; }
    public Car Car { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Payment? Payment { get; set; }
}