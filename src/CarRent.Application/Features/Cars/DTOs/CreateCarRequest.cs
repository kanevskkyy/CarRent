using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Http;
using DriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Application.DTOs.Cars;

public class CreateCarRequest
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal DailyPrice { get; set; }

    public BodyType BodyType { get; set; }
    public EngineType EngineType { get; set; }
    public DriveType DriveType { get; set; }
    public TransmissionType TransmissionType { get; set; }

    public int HorsePower { get; set; }
    public int SeatsCount { get; set; }
    public int TrunkVolumeLiters { get; set; }

    public IFormFile Photo { get; set; } = null!;
}