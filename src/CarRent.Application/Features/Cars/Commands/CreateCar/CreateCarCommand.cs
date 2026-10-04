using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Cars;

namespace CarRent.Application.Cars.Commands.CreateCar;

public record CreateCarCommand(CreateCarRequest Request, Stream PhotoStream, string FileName) : ICommand<CarDetailResponse>;