namespace CarRent.Application.DTOs.Bookings;

public class DailyBookingCountResponse
{
    public string Day { get; set; } = string.Empty; 
    public int Count { get; set; }
}