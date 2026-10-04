using AutoMapper;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Favorites.Queries.GetFavoriteCars;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Pagination;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Favorites;

public class GetFavoriteCarsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly Mock<IFavoriteCarStore> _favoriteStoreMock;
    private readonly IMapper _mapper;
    private readonly GetFavoriteCarsQueryHandler _handler;

    public GetFavoriteCarsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _favoriteStoreMock = new Mock<IFavoriteCarStore>();
        _mapper = TestHelper.CreateMapper();

        _handler = new GetFavoriteCarsQueryHandler(
            _unitOfWorkMock.Object,
            _favoriteStoreMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenNoFavoriteIds_ReturnsEmptyPagedList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetFavoriteCarsQuery query = new(userId, 1, 10);

        _favoriteStoreMock.Setup(f => f.GetPageAsync(userId, 1, 10))
            .ReturnsAsync((new List<Guid>(), 0L));

        // Act
        PagedList<CarListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _carRepoMock.Verify(c => c.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFavoritesExist_ReturnsOrderedCars()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Car car1 = TestHelper.CreateTestCar(name: "First Car");
        Car car2 = TestHelper.CreateTestCar(name: "Second Car");
        List<Guid> ids = new() { car2.Id, car1.Id }; // Specific order
        GetFavoriteCarsQuery query = new(userId, 1, 10);

        _favoriteStoreMock.Setup(f => f.GetPageAsync(userId, 1, 10))
            .ReturnsAsync((ids, 2L));
        _carRepoMock.Setup(c => c.GetByIdsAsync(It.Is<List<Guid>>(l => l.Contains(car1.Id) && l.Contains(car2.Id)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Car> { car1, car2 });

        // Act
        PagedList<CarListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(car2.Id);
        result.Items[1].Id.Should().Be(car1.Id);
        _favoriteStoreMock.Verify(f => f.RemoveManyAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSomeFavoritesMissingInDb_CleansUpMissingIdsAndAdjustsCount()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Car car1 = TestHelper.CreateTestCar();
        Guid deletedCarId = Guid.NewGuid();
        List<Guid> ids = new() { car1.Id, deletedCarId };
        GetFavoriteCarsQuery query = new(userId, 1, 10);

        _favoriteStoreMock.Setup(f => f.GetPageAsync(userId, 1, 10))
            .ReturnsAsync((ids, 2L));
        _carRepoMock.Setup(c => c.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Car> { car1 });

        // Act
        PagedList<CarListResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1); // 2 - 1 missing
        _favoriteStoreMock.Verify(f => f.RemoveManyAsync(userId, It.Is<IReadOnlyCollection<Guid>>(m => m.Contains(deletedCarId))), Times.Once);
    }
}
