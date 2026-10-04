using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Tests.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SqlServerApiCollection : ICollectionFixture<SqlServerApiFactory>
{
    public const string Name = "SQL Server API";
}

[Collection(SqlServerApiCollection.Name)]
public sealed class CarApiIntegrationTests
{
    private readonly SqlServerApiFactory _factory;

    public CarApiIntegrationTests(SqlServerApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPagedCars_FiltersAndPaginatesPersistedCars()
    {
        await ResetDataAsync(
            CreateCar("Sedan one", 80m, BodyType.Sedan),
            CreateCar("Sedan two", 120m, BodyType.Sedan),
            CreateCar("Hatchback", 95m, BodyType.Hatchback));

        using HttpResponseMessage response = await _factory.Client.GetAsync("/api/cars?BodyType=Sedan&PageNumber=1&PageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("totalCount").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("currentPage").GetInt32().Should().Be(1);
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("items")[0].GetProperty("name").GetString().Should().Be("Sedan one");
    }

    [Fact]
    public async Task GetCarById_ReturnsPersistedCar_AndNotFoundForUnknownId()
    {
        Car car = CreateCar("Persisted car", 145m, BodyType.Crossover);
        await ResetDataAsync(car);

        using HttpResponseMessage found = await _factory.Client.GetAsync($"/api/cars/{car.Id}");
        found.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument foundJson = JsonDocument.Parse(await found.Content.ReadAsStringAsync());
        foundJson.RootElement.GetProperty("id").GetGuid().Should().Be(car.Id);
        foundJson.RootElement.GetProperty("name").GetString().Should().Be(car.Name);

        using HttpResponseMessage missing = await _factory.Client.GetAsync($"/api/cars/{Guid.NewGuid()}");
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_RejectsInvalidRequest()
    {
        await ResetDataAsync();
        using HttpResponseMessage response = await _factory.Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "not-an-email",
            Password = "weak",
            FullName = "X"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Auth_RegisterLoginRefreshAndLogout_UsesPersistedRefreshTokens()
    {
        await ResetDataAsync();
        RegisteredUser user = await RegisterUserAsync();

        using HttpResponseMessage login = await _factory.Client.PostAsJsonAsync("/api/auth/login", new
        {
            user.Email,
            user.Password
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        string loginRefreshToken = loginJson.RootElement.GetProperty("refreshToken").GetString()!;

        using HttpResponseMessage refresh = await _factory.Client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = loginRefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument refreshJson = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync());
        string refreshedToken = refreshJson.RootElement.GetProperty("refreshToken").GetString()!;

        using HttpResponseMessage logout = await _factory.Client.PostAsJsonAsync("/api/auth/logout", new { RefreshToken = refreshedToken });
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using HttpResponseMessage revokedRefresh = await _factory.Client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = refreshedToken });
        revokedRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using IServiceScope scope = _factory.Services.CreateScope();
        CarRentDbContext db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        (await db.RefreshTokens.CountAsync(token => token.UserId == user.Id)).Should().Be(3);
    }

    [Fact]
    public async Task Bookings_CreateRejectOverlapListCancelAndAllowRebooking()
    {
        Car car = CreateCar("Booking car", 100m, BodyType.Sedan);
        await ResetDataAsync(car);
        RegisteredUser user = await RegisterUserAsync();
        Authenticate(user.AccessToken);

        DateTime startDate = DateTime.UtcNow.Date.AddDays(5);
        DateTime endDate = startDate.AddDays(4);
        object bookingRequest = new { CarId = car.Id, StartDate = startDate, EndDate = endDate };

        using HttpResponseMessage created = await _factory.Client.PostAsJsonAsync("/api/bookings", bookingRequest);
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Guid bookingId = createdJson.RootElement.GetProperty("id").GetGuid();
        createdJson.RootElement.GetProperty("totalPrice").GetDecimal().Should().Be(400m);
        createdJson.RootElement.GetProperty("status").GetString().Should().Be("Pending");

        using HttpResponseMessage overlap = await _factory.Client.PostAsJsonAsync("/api/bookings", bookingRequest);
        overlap.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        using HttpResponseMessage list = await _factory.Client.GetAsync("/api/bookings");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument listJson = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        listJson.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);

        using HttpResponseMessage cancelled = await _factory.Client.PostAsync($"/api/bookings/{bookingId}/cancel", null);
        cancelled.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using HttpResponseMessage rebooked = await _factory.Client.PostAsJsonAsync("/api/bookings", bookingRequest);
        rebooked.StatusCode.Should().Be(HttpStatusCode.OK);

        using IServiceScope scope = _factory.Services.CreateScope();
        CarRentDbContext db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        (await db.Bookings.CountAsync()).Should().Be(2);
        (await db.Bookings.CountAsync(booking => booking.Status == BookingStatus.Cancelled)).Should().Be(1);
    }

    [Fact]
    public async Task CreateBooking_ReturnsNotFoundForUnknownCar()
    {
        await ResetDataAsync();
        RegisteredUser user = await RegisterUserAsync();
        Authenticate(user.AccessToken);
        DateTime startDate = DateTime.UtcNow.Date.AddDays(3);

        using HttpResponseMessage response = await _factory.Client.PostAsJsonAsync("/api/bookings", new
        {
            CarId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = startDate.AddDays(1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBookings_RequiresAuthentication()
    {
        await ResetDataAsync();
        _factory.Client.DefaultRequestHeaders.Authorization = null;

        using HttpResponseMessage bookings = await _factory.Client.GetAsync("/api/bookings");
        bookings.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using HttpResponseMessage payment = await _factory.Client.PostAsync($"/api/payments/bookings/{Guid.NewGuid()}/pay", null);
        payment.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDailyBookingStats_IncludesCountsFromSqlServer()
    {
        Car car = CreateCar("Stats car", 90m, BodyType.Crossover);
        await ResetDataAsync(car);
        RegisteredUser user = await RegisterUserAsync();
        DateTime startDate = DateTime.UtcNow.Date.AddDays(2);

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            CarRentDbContext db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
            db.Bookings.Add(new Booking
            {
                UserId = user.Id,
                CarId = car.Id,
                StartDate = startDate,
                EndDate = startDate.AddDays(1),
                Status = BookingStatus.Confirmed,
                TotalPrice = 90m
            });
            await db.SaveChangesAsync();
        }

        using HttpResponseMessage response = await _factory.Client.GetAsync("/api/bookings/stats/daily");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetArrayLength().Should().Be(7);
        json.RootElement.EnumerateArray()
            .Single(day => day.GetProperty("day").GetString() == DateTime.UtcNow.DayOfWeek.ToString())
            .GetProperty("count").GetInt32().Should().Be(1);
    }

    private async Task ResetDataAsync(params Car[] cars)
    {
        _factory.Client.DefaultRequestHeaders.Authorization = null;
        using IServiceScope scope = _factory.Services.CreateScope();
        CarRentDbContext db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        await db.Payments.ExecuteDeleteAsync();
        await db.Bookings.ExecuteDeleteAsync();
        await db.Cars.ExecuteDeleteAsync();
        await db.RefreshTokens.ExecuteDeleteAsync();
        if (cars.Length == 0)
            return;
        db.Cars.AddRange(cars);
        await db.SaveChangesAsync();
    }

    private async Task<RegisteredUser> RegisterUserAsync()
    {
        string email = $"integration-{Guid.NewGuid():N}@example.test";
        const string password = "Strong!Password123";
        using HttpResponseMessage response = await _factory.Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            FullName = "Integration User",
            DriverLicenseNumber = "DL-12345"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync() + Environment.NewLine + string.Join(Environment.NewLine, _factory.CapturedLogs));
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = json.RootElement;
        return new RegisteredUser(
            root.GetProperty("user").GetProperty("id").GetGuid(),
            email,
            password,
            root.GetProperty("accessToken").GetString()!);
    }

    private void Authenticate(string accessToken) =>
        _factory.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    private static Car CreateCar(string name, decimal price, BodyType bodyType) => new()
    {
        Name = name,
        Year = 2024,
        DailyPrice = price,
        BodyType = bodyType,
        EngineType = EngineType.Petrol,
        DriveType = DriveType.FrontWheel,
        TransmissionType = TransmissionType.Automatic,
        HorsePower = 180,
        PhotoUrl = "https://example.test/car.jpg",
        SeatsCount = 5,
        TrunkVolumeLiters = 400
    };

    private sealed record RegisteredUser(Guid Id, string Email, string Password, string AccessToken);
}
