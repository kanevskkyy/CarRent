namespace CarRent.Application.DTOs.Cars;

public class CarBookingResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}