using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;

namespace CarRent.Application.Cars.Commands.UpdateCar;

public class UpdateCarCommandHandler : ICommandHandler<UpdateCarCommand, CarDetailResponse?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageUploaderService _imageUploaderService;
    private readonly IMapper _mapper;

    public UpdateCarCommandHandler(
        IUnitOfWork unitOfWork,
        IImageUploaderService imageUploaderService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _imageUploaderService = imageUploaderService;
        _mapper = mapper;
    }

    public async Task<CarDetailResponse?> Handle(UpdateCarCommand command, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetByIdAsync(command.Id, cancellationToken);
        if (car == null)
            throw new NotFoundException($"Car with  id: {command.Id} not found");

        _mapper.Map(command.Request, car);

        if (command.PhotoStream != null && !string.IsNullOrEmpty(command.FileName))
        {
            string newPhotoUrl = await _imageUploaderService.UploadAsync(
                command.PhotoStream, 
                command.FileName, 
                cancellationToken);
                
            car.PhotoUrl = newPhotoUrl;
        }

        _unitOfWork.Cars.Update(car);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CarDetailResponse>(car);
    }
}