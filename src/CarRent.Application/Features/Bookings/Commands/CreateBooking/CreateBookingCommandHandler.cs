using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingListResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public CreateBookingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<BookingListResponse> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetByIdAsync(command.Request.CarId, cancellationToken);
        if (car == null)
            throw new NotFoundException($"Car with id {command.Request.CarId} not found.");

        bool isAvailable = await _unitOfWork.Bookings.IsCarAvailableAsync(
            command.Request.CarId, 
            command.Request.StartDate, 
            command.Request.EndDate, 
            cancellationToken);

        if (!isAvailable)
            throw new ValidationException("Car is not available for the selected dates.");
        
        User? user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null || user.LicenseStatus != DriverLicenseVerificationStatus.Verified)
            throw new ValidationException("Driver license must be verified before booking.");

        int totalDays = (int)Math.Ceiling((command.Request.EndDate - command.Request.StartDate).TotalDays);

        Booking booking = _mapper.Map<Booking>(command.Request);
        booking.UserId = command.UserId;
        booking.TotalPrice = totalDays * car.DailyPrice;

        await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        booking.Car = car;
        return _mapper.Map<BookingListResponse>(booking);
    }
}