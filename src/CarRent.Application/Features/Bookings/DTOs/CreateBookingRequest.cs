namespace CarRent.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}