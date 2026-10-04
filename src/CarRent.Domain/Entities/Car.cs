using CarRent.Domain.Enums;
using DriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Domain.Entities;

public class Car
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal DailyPrice { get; set; }

    public BodyType BodyType { get; set; }
    public EngineType EngineType { get; set; }
    public DriveType DriveType { get; set; }
    public TransmissionType TransmissionType { get; set; }

    public int HorsePower { get; set; }
    
    public string PhotoUrl { get; set; } = string.Empty;
    public int SeatsCount { get; set; }
    public int TrunkVolumeLiters { get; set; }
    
    public List<Booking> Bookings { get; set; } = new();
}