using AutoMapper;
using CarRent.Application.Bookings.Queries.GetPagedBookings;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Bookings;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Bookings;

public class GetPagedBookingsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly IMapper _mapper;
    private readonly GetPagedBookingsQueryHandler _handler;

    public GetPagedBookingsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _mapper = TestHelper.CreateMapper();
        _handler = new GetPagedBookingsQueryHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_EnforcesUserIdOnFilter()
    {
        // Arrange
        Guid currentUserId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        BookingFilterModel filter = new() { PageNumber = 1, PageSize = 10, UserId = otherUserId };
        GetPagedBookingsQuery query = new(filter, currentUserId, false);

        Booking booking = TestHelper.CreateTestBooking(userId: currentUserId);
        booking.Car = TestHelper.CreateTestCar();
        PagedList<Booking> pagedBookings = new(new List<Booking> { booking }, 1, 1, 10);

        _bookingRepoMock.Setup(b => b.GetPagedBookingsWithFiltersAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedBookings);

        // Act
        PagedList<BookingListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        filter.UserId.Should().Be(currentUserId);
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_PreservesRequestedUserIdFilter()
    {
        // Arrange
        Guid adminId = Guid.NewGuid();
        Guid specificUserId = Guid.NewGuid();
        BookingFilterModel filter = new() { PageNumber = 1, PageSize = 10, UserId = specificUserId };
        GetPagedBookingsQuery query = new(filter, adminId, true);

        Booking booking = TestHelper.CreateTestBooking(userId: specificUserId);
        booking.Car = TestHelper.CreateTestCar();
        PagedList<Booking> pagedBookings = new(new List<Booking> { booking }, 1, 1, 10);

        _bookingRepoMock.Setup(b => b.GetPagedBookingsWithFiltersAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedBookings);

        // Act
        PagedList<BookingListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        filter.UserId.Should().Be(specificUserId);
        result.Items.Should().HaveCount(1);
    }
}
