using AutoMapper;
using CarRent.Application.Mappings;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using DomainDriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Tests.Common;

public static class TestHelper
{
    private static IMapper? _mapper;

    public static IMapper CreateMapper()
    {
        if (_mapper != null) return _mapper;

        ServiceCollection services = new();
        services.AddLogging();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(CarMappingProfile).Assembly);
        });

        ServiceProvider sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
        return _mapper;
    }

    public static Mock<UserManager<User>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static User CreateTestUser(
        Guid? id = null,
        string email = "test@example.com",
        string fullName = "Test User",
        DriverLicenseVerificationStatus licenseStatus = DriverLicenseVerificationStatus.Verified)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            UserName = email,
            FullName = fullName,
            DriverLicenseNumber = "DL-123456",
            LicenseStatus = licenseStatus,
            EmailConfirmed = true
        };
    }

    public static Car CreateTestCar(
        Guid? id = null,
        string name = "Tesla Model 3",
        decimal dailyPrice = 100m,
        int year = 2024)
    {
        return new Car
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            DailyPrice = dailyPrice,
            Year = year,
            EngineType = EngineType.Electric,
            TransmissionType = TransmissionType.Automatic,
            DriveType = DomainDriveType.AllWheel,
            BodyType = BodyType.Sedan,
            SeatsCount = 5,
            TrunkVolumeLiters = 425,
            HorsePower = 283,
            PhotoUrl = "https://example.com/photo.jpg"
        };
    }

    public static Booking CreateTestBooking(
        Guid? id = null,
        Guid? carId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        BookingStatus status = BookingStatus.Pending,
        decimal totalPrice = 300m)
    {
        DateTime start = startDate ?? DateTime.UtcNow.AddDays(1);
        DateTime end = endDate ?? start.AddDays(3);

        return new Booking
        {
            Id = id ?? Guid.NewGuid(),
            CarId = carId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            StartDate = start,
            EndDate = end,
            Status = status,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };
    }
}
