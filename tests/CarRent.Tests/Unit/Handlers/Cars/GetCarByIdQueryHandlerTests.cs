using AutoMapper;
using CarRent.Application.Cars.Queries.GetCarById;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Cars;

public class GetCarByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly GetCarByIdQueryHandler _handler;

    public GetCarByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _mapper = TestHelper.CreateMapper();
        _userManagerMock = TestHelper.CreateMockUserManager();

        _handler = new GetCarByIdQueryHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid carId = Guid.NewGuid();
        GetCarByIdQuery query = new(carId);

        _carRepoMock.Setup(c => c.GetCarByIdWithBookingsAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Car with id: {carId} not found");
    }

    [Fact]
    public async Task Handle_WhenNoUserSpecified_ReturnsCarWithoutCheckingLicense()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        GetCarByIdQuery query = new(car.Id, null);

        _carRepoMock.Setup(c => c.GetCarByIdWithBookingsAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);

        // Act
        CarDetailResponse? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(car.Id);
        result.Name.Should().Be(car.Name);
        result.IsDriverLicenseVerified.Should().BeFalse();
        _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserSpecifiedButNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        Guid userId = Guid.NewGuid();
        GetCarByIdQuery query = new(car.Id, userId);

        _carRepoMock.Setup(c => c.GetCarByIdWithBookingsAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with id: {userId} not found");
    }

    [Fact]
    public async Task Handle_WhenUserHasVerifiedLicense_SetsIsDriverLicenseVerifiedTrue()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        User user = TestHelper.CreateTestUser(licenseStatus: DriverLicenseVerificationStatus.Verified);
        GetCarByIdQuery query = new(car.Id, user.Id);

        _carRepoMock.Setup(c => c.GetCarByIdWithBookingsAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        // Act
        CarDetailResponse? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsDriverLicenseVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserHasPendingLicense_SetsIsDriverLicenseVerifiedFalse()
    {
        // Arrange
        Car car = TestHelper.CreateTestCar();
        User user = TestHelper.CreateTestUser(licenseStatus: DriverLicenseVerificationStatus.Pending);
        GetCarByIdQuery query = new(car.Id, user.Id);

        _carRepoMock.Setup(c => c.GetCarByIdWithBookingsAsync(car.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(car);
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        // Act
        CarDetailResponse? result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsDriverLicenseVerified.Should().BeFalse();
    }
}
