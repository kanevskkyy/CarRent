using CarRent.Application.Abstractions;

namespace CarRent.Application.Favorites.Commands.AddFavorite;

public record AddFavoriteCommand(Guid UserId, Guid CarId) : ICommand;