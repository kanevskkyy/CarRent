using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;

namespace CarRent.Application.Features.Cars.Queries.GetPopularCars;

public class GetPopularCarsQueryHandler : IQueryHandler<GetPopularCarsQuery, List<CarListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPopularCarsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CarListResponse>> Handle(
        GetPopularCarsQuery request, CancellationToken cancellationToken)
    {
        int count = Math.Clamp(request.Count, 1, 10);

        IReadOnlyList<Car> cars = await _unitOfWork.Bookings.GetMostBookedCarsAsync(count, cancellationToken);

        return _mapper.Map<List<CarListResponse>>(cars);
    }
}