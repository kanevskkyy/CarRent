using AutoMapper;
using CarRent.Application.Bookings.Queries.GetDailyBookingStats;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Bookings;

public class GetDailyBookingStatsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly IMapper _mapper;
    private readonly GetDailyBookingStatsQueryHandler _handler;

    public GetDailyBookingStatsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _mapper = TestHelper.CreateMapper();
        _handler = new GetDailyBookingStatsQueryHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WhenCalled_ReturnsMappedDailyBookingStats()
    {
        // Arrange
        Dictionary<string, int> stats = new()
        {
            { "Monday", 5 },
            { "Tuesday", 3 },
            { "Wednesday", 0 }
        };
        _bookingRepoMock.Setup(b => b.GetDailyBookingCountsForLast48HoursAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        List<DailyBookingCountResponse> result = await _handler.Handle(new GetDailyBookingStatsQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Day == "Monday" && r.Count == 5);
        result.Should().Contain(r => r.Day == "Tuesday" && r.Count == 3);
        result.Should().Contain(r => r.Day == "Wednesday" && r.Count == 0);
    }
}
