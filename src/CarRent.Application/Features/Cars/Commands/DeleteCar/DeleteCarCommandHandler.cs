using CarRent.Application.Abstractions;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using MediatR;

namespace CarRent.Application.Cars.Commands.DeleteCar;

public class DeleteCarCommandHandler : ICommandHandler<DeleteCarCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCarCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCarCommand command, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetByIdAsync(command.Id, cancellationToken);
        if (car == null)
            throw new NotFoundException($"Car with  id: {command.Id} not found");

        _unitOfWork.Cars.Delete(car);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}