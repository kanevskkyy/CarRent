using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Cars;

namespace CarRent.Application.Cars.Commands.UpdateCar;

public record UpdateCarCommand(
    Guid Id, 
    UpdateCarRequest Request, 
    Stream? PhotoStream = null, 
    string? FileName = null) : ICommand<CarDetailResponse?>;