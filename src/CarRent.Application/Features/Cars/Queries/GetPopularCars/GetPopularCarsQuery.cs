using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;

namespace CarRent.Application.Features.Cars.Queries.GetPopularCars;

public record GetPopularCarsQuery(int Count = 3) : IQuery<List<CarListResponse>>;