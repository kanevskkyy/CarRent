using CarRent.Application.Abstractions;

namespace CarRent.Application.Favorites.Commands.RemoveFavorite;

public record RemoveFavoriteCommand(Guid UserId, Guid CarId) : ICommand;