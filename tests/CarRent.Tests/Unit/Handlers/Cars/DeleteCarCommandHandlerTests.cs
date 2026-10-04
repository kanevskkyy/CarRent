using CarRent.Application.Cars.Commands.DeleteCar;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Cars;

public class DeleteCarCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly DeleteCarCommandHandler _handler;

    public DeleteCarCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);

        _handler = new DeleteCarCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid carId = Guid.NewGuid();
        DeleteCarCommand command = new(carId);

        _carRepoMock.Setup(c => c.GetByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{carId}*");
    }

    [Fact]
    public async Task Handle_WhenCarFound_DeletesAndSavesChanges()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        DeleteCarCommand command = new(car.Id);

        _carRepoMock.Setup(c => c.GetByIdAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _carRepoMock.Verify(c => c.Delete(car), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
