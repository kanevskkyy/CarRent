using CarRent.Application.Abstractions;
using CarRent.Application.Interfaces.Services;
using MediatR;

namespace CarRent.Application.Favorites.Commands.RemoveFavorite;

public class RemoveFavoriteCommandHandler : ICommandHandler<RemoveFavoriteCommand>
{
    private readonly IFavoriteCarStore _favorites;

    public RemoveFavoriteCommandHandler(IFavoriteCarStore favorites)
    {
        _favorites = favorites;
    }

    public async Task<Unit> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        await _favorites.RemoveAsync(command.UserId, command.CarId);

        return Unit.Value;
    }
}