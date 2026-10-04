using AutoMapper;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.DTOs.Cars;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CarRent.Tests.Unit.Mappings;

public class MappingTests
{
    private readonly IMapper _mapper = TestHelper.CreateMapper();

    [Fact]
    public void Mapper_Configuration_ShouldBeValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_Map_Car_To_CarListResponse()
    {
        Car car = TestHelper.CreateTestCar();

        CarListResponse response = _mapper.Map<CarListResponse>(car);

        response.Should().NotBeNull();
        response.Id.Should().Be(car.Id);
        response.Name.Should().Be(car.Name);
        response.DailyPrice.Should().Be(car.DailyPrice);
        response.PhotoUrl.Should().Be(car.PhotoUrl);
    }

    [Fact]
    public void Should_Map_Booking_To_BookingListResponse()
    {
        Booking booking = TestHelper.CreateTestBooking();
        booking.Car = TestHelper.CreateTestCar();

        BookingListResponse response = _mapper.Map<BookingListResponse>(booking);

        response.Should().NotBeNull();
        response.Id.Should().Be(booking.Id);
        response.Car.Should().NotBeNull();
        response.Car.Name.Should().Be(booking.Car.Name);
    }

    [Fact]
    public void Should_Map_User_To_UserResponse()
    {
        User user = TestHelper.CreateTestUser();

        UserResponse response = _mapper.Map<UserResponse>(user);

        response.Should().NotBeNull();
        response.Id.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        response.FullName.Should().Be(user.FullName);
    }
}
