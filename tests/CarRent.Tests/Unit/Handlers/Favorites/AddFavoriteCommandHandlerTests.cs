using CarRent.Application.Exceptions;
using CarRent.Application.Favorites.Commands.AddFavorite;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Favorites;

public class AddFavoriteCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly Mock<IFavoriteCarStore> _favoriteStoreMock;
    private readonly AddFavoriteCommandHandler _handler;

    public AddFavoriteCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _favoriteStoreMock = new Mock<IFavoriteCarStore>();

        _handler = new AddFavoriteCommandHandler(_unitOfWorkMock.Object, _favoriteStoreMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid carId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        AddFavoriteCommand command = new(userId, carId);

        _carRepoMock.Setup(c => c.GetByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Car with id {carId} not found.");
    }

    [Fact]
    public async Task Handle_WhenCarExists_AddsToFavoriteStore()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        Guid userId = Guid.NewGuid();
        AddFavoriteCommand command = new(userId, car.Id);

        _carRepoMock.Setup(c => c.GetByIdAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _favoriteStoreMock.Verify(f => f.AddAsync(userId, car.Id), Times.Once);
    }
}
