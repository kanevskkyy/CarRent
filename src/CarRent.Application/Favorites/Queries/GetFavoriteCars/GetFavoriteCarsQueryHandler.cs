using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Favorites.Queries.GetFavoriteCars;

public class GetFavoriteCarsQueryHandler : IQueryHandler<GetFavoriteCarsQuery, PagedList<CarListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFavoriteCarStore _favorites;
    private readonly IMapper _mapper;

    public GetFavoriteCarsQueryHandler(IUnitOfWork unitOfWork, IFavoriteCarStore favorites, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _favorites = favorites;
        _mapper = mapper;
    }

    public async Task<PagedList<CarListResponse>> Handle(
        GetFavoriteCarsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = Math.Max(1, request.PageNumber);
        int pageSize = Math.Clamp(request.PageSize, 1, 50);

        (IReadOnlyList<Guid> ids, long total) = await _favorites.GetPageAsync(request.UserId, pageNumber, pageSize);

        if (ids.Count == 0)
            return new PagedList<CarListResponse>(new List<CarListResponse>(), (int)total, pageNumber, pageSize);

        IReadOnlyList<Car> cars = await _unitOfWork.Cars.GetByIdsAsync(ids.ToList(), cancellationToken);
        Dictionary<Guid, Car> carsById = cars.ToDictionary(c => c.Id);

        List<Car> ordered = ids.Where(carsById.ContainsKey).Select(id => carsById[id]).ToList();

        List<Guid> missing = ids.Where(id => !carsById.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            await _favorites.RemoveManyAsync(request.UserId, missing);

        List<CarListResponse> mapped = _mapper.Map<List<CarListResponse>>(ordered);

        return new PagedList<CarListResponse>(mapped, (int)total - missing.Count, pageNumber, pageSize);
    }
}