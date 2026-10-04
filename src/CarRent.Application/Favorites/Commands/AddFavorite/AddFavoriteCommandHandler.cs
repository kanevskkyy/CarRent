using CarRent.Application.Abstractions;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using MediatR;

namespace CarRent.Application.Favorites.Commands.AddFavorite;

public class AddFavoriteCommandHandler : ICommandHandler<AddFavoriteCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFavoriteCarStore _favorites;

    public AddFavoriteCommandHandler(IUnitOfWork unitOfWork, IFavoriteCarStore favorites)
    {
        _unitOfWork = unitOfWork;
        _favorites = favorites;
    }

    public async Task<Unit> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        Car? car = await _unitOfWork.Cars.GetByIdAsync(command.CarId, cancellationToken);
        if (car is null)
            throw new NotFoundException($"Car with id {command.CarId} not found.");

        await _favorites.AddAsync(command.UserId, command.CarId);

        return Unit.Value;
    }
}