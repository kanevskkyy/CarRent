using CarRent.Application.Abstractions;
using MediatR;

namespace CarRent.Application.Cars.Commands.DeleteCar;

public record DeleteCarCommand(Guid Id) : ICommand<Unit>;