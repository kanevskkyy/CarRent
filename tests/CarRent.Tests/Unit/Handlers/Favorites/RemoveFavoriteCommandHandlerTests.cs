using CarRent.Application.Favorites.Commands.RemoveFavorite;
using CarRent.Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Favorites;

public class RemoveFavoriteCommandHandlerTests
{
    private readonly Mock<IFavoriteCarStore> _favoriteStoreMock;
    private readonly RemoveFavoriteCommandHandler _handler;

    public RemoveFavoriteCommandHandlerTests()
    {
        _favoriteStoreMock = new Mock<IFavoriteCarStore>();
        _handler = new RemoveFavoriteCommandHandler(_favoriteStoreMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_RemovesFromFavoriteStore()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid carId = Guid.NewGuid();
        RemoveFavoriteCommand command = new(userId, carId);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _favoriteStoreMock.Verify(f => f.RemoveAsync(userId, carId), Times.Once);
    }
}
