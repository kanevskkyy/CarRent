using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;

namespace CarRent.Application.Cars.Queries.GetCarById;

public class GetCarByIdQueryHandler : IQueryHandler<GetCarByIdQuery, CarDetailResponse?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCarByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CarDetailResponse?> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetCarByIdWithBookingsAsync(request.CarId, cancellationToken);
        if (car == null)
            throw new NotFoundException($"Car with  id: {request.CarId} not found");

        CarDetailResponse response = _mapper.Map<CarDetailResponse>(car);

        if (request.CurrentUserId.HasValue)
        {
            // TODO: Inject UserManager<User> or ICurrentUserContext service here to check driver license status via ASP.NET Core Identity.
        }

        return response;
    }
}