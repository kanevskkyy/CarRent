using AutoMapper;
using CarRent.Application.Bookings.Commands.CreateBooking;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Bookings;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly IMapper _mapper;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);

        _mapper = TestHelper.CreateMapper();
        _handler = new CreateBookingCommandHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid carId = Guid.NewGuid();
        CreateBookingRequest request = new()
        {
            CarId = carId,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(4)
        };
        CreateBookingCommand command = new(Guid.NewGuid(), request);

        _carRepoMock.Setup(c => c.GetByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Car with id {carId} not found.");
    }

    [Fact]
    public async Task Handle_WhenCarNotAvailable_ThrowsValidationException()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        CreateBookingRequest request = new()
        {
            CarId = car.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(4)
        };
        CreateBookingCommand command = new(Guid.NewGuid(), request);

        _carRepoMock.Setup(c => c.GetByIdAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);
        _bookingRepoMock.Setup(b => b.IsCarAvailableAsync(car.Id, request.StartDate, request.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Car is not available for the selected dates.");
    }

    [Fact]
    public async Task Handle_WhenCarAvailable_CalculatesPriceAndCreatesBooking()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar(dailyPrice: 150m);
        DateTime startDate = DateTime.UtcNow.Date.AddDays(1);
        DateTime endDate = startDate.AddDays(3); // 3 days
        Guid userId = Guid.NewGuid();

        CreateBookingRequest request = new()
        {
            CarId = car.Id,
            StartDate = startDate,
            EndDate = endDate
        };
        CreateBookingCommand command = new(userId, request);

        _carRepoMock.Setup(c => c.GetByIdAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);
        _bookingRepoMock.Setup(b => b.IsCarAvailableAsync(car.Id, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Booking? savedBooking = null;
        _bookingRepoMock.Setup(b => b.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, _) => savedBooking = b)
            .Returns(Task.CompletedTask);

        // Act
        BookingListResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalPrice.Should().Be(450m); // 3 * 150
        result.Car.Should().NotBeNull();
        result.Car.Name.Should().Be(car.Name);

        savedBooking.Should().NotBeNull();
        savedBooking!.UserId.Should().Be(userId);
        savedBooking.TotalPrice.Should().Be(450m);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
