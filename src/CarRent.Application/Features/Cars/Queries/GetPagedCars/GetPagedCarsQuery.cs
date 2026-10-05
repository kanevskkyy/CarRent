using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Models.Cars;
using CarRent.Application.Models.Pagination;

namespace CarRent.Application.Cars.Queries.GetPagedCars;

public record GetPagedCarsQuery(CarFilterModel Filter, Guid UserId = default) : IQuery<PagedList<CarListResponse>>;