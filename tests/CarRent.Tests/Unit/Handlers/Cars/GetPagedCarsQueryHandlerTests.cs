using AutoMapper;
using CarRent.Application.Cars.Queries.GetPagedCars;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
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
    private readonly Mock<IFavoriteCarStore> _favoritesMock;

    public GetPagedCarsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _favoritesMock = new Mock<IFavoriteCarStore>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _mapper = TestHelper.CreateMapper();

        _handler = new GetPagedCarsQueryHandler(_unitOfWorkMock.Object, _mapper, _favoritesMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenUserIsLoggedIn_MarksFavoriteCars()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Car liked = TestHelper.CreateTestCar();
        Car notLiked = TestHelper.CreateTestCar();
        CarFilterModel filter = new();

        _carRepoMock
            .Setup(c => c.GetPagedCarsWithFiltersAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Car>(new List<Car> { liked, notLiked }, 2, 1, 10));

        _favoritesMock
            .Setup(f => f.GetFavoriteIdsAsync(userId, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new HashSet<Guid> { liked.Id });

        // Act
        PagedList<CarListResponse> result =
            await _handler.Handle(new GetPagedCarsQuery(filter, userId), CancellationToken.None);

        // Assert
        result.Items.Single(c => c.Id == liked.Id).IsFavorite.Should().BeTrue();
        result.Items.Single(c => c.Id == notLiked.Id).IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_DoesNotCallRedis()
    {
        // Arrange
        CarFilterModel filter = new();
        _carRepoMock
            .Setup(c => c.GetPagedCarsWithFiltersAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Car>(new List<Car> { TestHelper.CreateTestCar() }, 1, 1, 10));

        // Act
        PagedList<CarListResponse> result =
            await _handler.Handle(new GetPagedCarsQuery(filter), CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(c => !c.IsFavorite);
        _favoritesMock.Verify(
            f => f.GetFavoriteIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
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
