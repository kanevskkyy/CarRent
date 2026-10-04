using CarRent.Domain.Enums;

namespace CarRent.Application.DTOs.Cars;

public class CarListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public BodyType BodyType { get; set; }
    public TransmissionType TransmissionType { get; set; }
    public EngineType EngineType { get; set; }
    public int SeatsCount { get; set; }
    public int Year { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}