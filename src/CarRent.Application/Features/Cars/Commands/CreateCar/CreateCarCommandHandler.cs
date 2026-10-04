using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;

namespace CarRent.Application.Cars.Commands.CreateCar;

public class CreateCarCommandHandler : ICommandHandler<CreateCarCommand, CarDetailResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageUploaderService _imageUploaderService;
    private readonly IMapper _mapper;

    public CreateCarCommandHandler(
        IUnitOfWork unitOfWork,
        IImageUploaderService imageUploaderService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _imageUploaderService = imageUploaderService;
        _mapper = mapper;
    }

    public async Task<CarDetailResponse> Handle(CreateCarCommand command, CancellationToken cancellationToken)
    {
        string photoUrl = await _imageUploaderService.UploadAsync(
            command.PhotoStream, 
            command.FileName, 
            cancellationToken);

        Car car = _mapper.Map<Car>(command.Request);
        car.PhotoUrl = photoUrl;

        await _unitOfWork.Cars.AddAsync(car, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CarDetailResponse>(car);
    }
}