using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Models.Pagination;

namespace CarRent.Application.Favorites.Queries.GetFavoriteCars;

public record GetFavoriteCarsQuery(Guid UserId, int PageNumber, int PageSize)
    : IQuery<PagedList<CarListResponse>>;