using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;

namespace CarRent.Application.Cars.Queries.GetCarById;

public record GetCarByIdQuery(Guid CarId, Guid? CurrentUserId = null) : IQuery<CarDetailResponse?>;