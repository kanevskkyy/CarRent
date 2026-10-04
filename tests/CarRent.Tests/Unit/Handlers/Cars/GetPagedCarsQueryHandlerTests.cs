using AutoMapper;
using CarRent.Application.Cars.Queries.GetPagedCars;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Models.Cars;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Cars;

public class GetPagedCarsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly IMapper _mapper;
    private readonly GetPagedCarsQueryHandler _handler;

    public GetPagedCarsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _mapper = TestHelper.CreateMapper();

        _handler = new GetPagedCarsQueryHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WhenCalled_ReturnsPagedCarListResponse()
    {
        // Arrange
        CarFilterModel filter = new() { PageNumber = 1, PageSize = 10 };
        GetPagedCarsQuery query = new(filter);

        Car car1 = TestHelper.CreateTestCar(name: "BMW 3 Series");
        Car car2 = TestHelper.CreateTestCar(name: "Mercedes C-Class");
        PagedList<Car> pagedCars = new(new List<Car> { car1, car2 }, 2, 1, 10);

        _carRepoMock.Setup(c => c.GetPagedCarsWithFiltersAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedCars);

        // Act
        PagedList<CarListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("BMW 3 Series");
        result.Items[1].Name.Should().Be("Mercedes C-Class");
    }
}
