using CarRent.Domain.Enums;

namespace CarRent.Application.DTOs.Bookings;

public class BookingListResponse
{
    public Guid Id { get; set; }
    
    public BookingCarResponse Car { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; } 
    public PaymentStatus PaymentStatus { get; set; } 

    public DateTime CreatedAt { get; set; }
}