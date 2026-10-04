using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Cars.Queries.GetCarById;

public class GetCarByIdQueryHandler : IQueryHandler<GetCarByIdQuery, CarDetailResponse?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public GetCarByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<CarDetailResponse?> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetCarByIdWithBookingsAsync(request.CarId, cancellationToken);
        if (car == null)
            throw new NotFoundException($"Car with id: {request.CarId} not found");

        CarDetailResponse response = _mapper.Map<CarDetailResponse>(car);

        if (request.CurrentUserId.HasValue)
        {
            User user = await _userManager.FindByIdAsync(request.CurrentUserId.ToString());
            
            if (user == null) 
                throw new NotFoundException($"User with id: {request.CurrentUserId} not found");
            
            response.IsDriverLicenseVerified =
                user.LicenseStatus == DriverLicenseVerificationStatus.Verified;
        }

        return response;
    }
}