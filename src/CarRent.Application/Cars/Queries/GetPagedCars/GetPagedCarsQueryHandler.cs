using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;

namespace CarRent.Application.Cars.Queries.GetPagedCars;

public class GetPagedCarsQueryHandler : IQueryHandler<GetPagedCarsQuery, PagedList<CarListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPagedCarsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedList<CarListResponse>> Handle(GetPagedCarsQuery request, CancellationToken cancellationToken)
    {
        PagedList<Car> pagedCars = await _unitOfWork.Cars.GetPagedCarsWithFiltersAsync(request.Filter, cancellationToken);

        List<CarListResponse> mappedItems = _mapper.Map<List<CarListResponse>>(pagedCars.Items);

        return new PagedList<CarListResponse>(
            mappedItems, 
            pagedCars.TotalCount, 
            pagedCars.CurrentPage, 
            pagedCars.PageSize);
    }
}