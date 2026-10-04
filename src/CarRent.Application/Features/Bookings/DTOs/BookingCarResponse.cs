namespace CarRent.Application.DTOs.Bookings;

public class BookingCarResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}