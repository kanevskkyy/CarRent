using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Cars.Queries.GetPagedCars;

public class GetPagedCarsQueryHandler : IQueryHandler<GetPagedCarsQuery, PagedList<CarListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFavoriteCarStore _favorites;

    public GetPagedCarsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,  IFavoriteCarStore favorites)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _favorites = favorites;
    }

    public async Task<PagedList<CarListResponse>> Handle(GetPagedCarsQuery request, CancellationToken cancellationToken)
    {
        PagedList<Car> pagedCars = await _unitOfWork.Cars.GetPagedCarsWithFiltersAsync(request.Filter, cancellationToken);

        List<CarListResponse> mappedItems = _mapper.Map<List<CarListResponse>>(pagedCars.Items);

        if (request.UserId != Guid.Empty && mappedItems.Count > 0)
        {
            HashSet<Guid> favorites = await _favorites.GetFavoriteIdsAsync(
                request.UserId, mappedItems.Select(c => c.Id));

            foreach (CarListResponse item in mappedItems)
                item.IsFavorite = favorites.Contains(item.Id);
        }

        return new PagedList<CarListResponse>(
            mappedItems, 
            pagedCars.TotalCount, 
            pagedCars.CurrentPage, 
            pagedCars.PageSize);
    }
}