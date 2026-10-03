using System.Text.Json;
using System.Text.Json.Serialization;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Persistence.Seeders;

public class BookingSeeder : IDbSeeder
{
    private readonly CarRentDbContext _context;
    private readonly ILogger<BookingSeeder> _logger;

    public BookingSeeder(CarRentDbContext context, ILogger<BookingSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Bookings.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Bookings table already contains records. Skipping booking seeding.");
            return;
        }

        List<Car> cars = await _context.Cars.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        if (cars.Count == 0)
        {
            _logger.LogWarning("No cars available in database to attach bookings to. Skipping booking seeding.");
            return;
        }

        List<User> users = await EnsureUsersExistAsync(cancellationToken);
        if (users.Count == 0)
        {
            _logger.LogWarning("No users available to attach bookings to. Skipping booking seeding.");
            return;
        }

        string? filePath = ResolveDataFilePath("bookings.json");
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("bookings.json seed file was not found. Looked in AppContext.BaseDirectory and parent directories.");
            return;
        }

        _logger.LogInformation("Seeding bookings from {FilePath}...", filePath);
        string json = await File.ReadAllTextAsync(filePath, cancellationToken);

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        List<BookingSeedDto>? seedDtos = JsonSerializer.Deserialize<List<BookingSeedDto>>(json, options);
        if (seedDtos == null || seedDtos.Count == 0)
        {
            _logger.LogWarning("No booking items found in {FilePath} to seed.", filePath);
            return;
        }

        List<Booking> bookings = new();
        foreach (BookingSeedDto dto in seedDtos)
        {
            Car selectedCar = cars[dto.CarIndex % cars.Count];
            User selectedUser = users[dto.UserIndex % users.Count];

            DateTime createdAt = DateTime.UtcNow.AddHours(-dto.CreatedHoursAgo);
            DateTime startDate = DateTime.UtcNow.Date.AddDays(dto.StartOffsetDays);
            int duration = dto.DurationDays > 0 ? dto.DurationDays : 3;
            DateTime endDate = startDate.AddDays(duration);

            decimal calculatedPrice = dto.TotalPrice ?? (selectedCar.DailyPrice * duration);

            Booking booking = new()
            {
                Id = Guid.NewGuid(),
                CarId = selectedCar.Id,
                UserId = selectedUser.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = dto.Status,
                TotalPrice = calculatedPrice,
                CreatedAt = createdAt
            };

            bookings.Add(booking);
        }

        await _context.Bookings.AddRangeAsync(bookings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} bookings.", bookings.Count);
    }

    private async Task<List<User>> EnsureUsersExistAsync(CancellationToken cancellationToken)
    {
        List<User> existingUsers = await _context.Users.ToListAsync(cancellationToken);
        if (existingUsers.Count > 0)
        {
            return existingUsers;
        }

        List<User> defaultUsers =
        [
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "alexander.kovalenko@example.com",
                NormalizedUserName = "ALEXANDER.KOVALENKO@EXAMPLE.COM",
                Email = "alexander.kovalenko@example.com",
                NormalizedEmail = "ALEXANDER.KOVALENKO@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Alexander Kovalenko",
                DriverLicenseNumber = "BXT-998821",
                LicenseStatus = DriverLicenseVerificationStatus.Verified,
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "maksym.shevchenko@example.com",
                NormalizedUserName = "MAKSYM.SHEVCHENKO@EXAMPLE.COM",
                Email = "maksym.shevchenko@example.com",
                NormalizedEmail = "MAKSYM.SHEVCHENKO@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Maksym Shevchenko",
                DriverLicenseNumber = "CXM-445512",
                LicenseStatus = DriverLicenseVerificationStatus.Verified,
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new User
            {
                Id = Guid.NewGuid(),
                UserName = "olena.melnyk@example.com",
                NormalizedUserName = "OLENA.MELNYK@EXAMPLE.COM",
                Email = "olena.melnyk@example.com",
                NormalizedEmail = "OLENA.MELNYK@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Olena Melnyk",
                DriverLicenseNumber = "AKL-771190",
                LicenseStatus = DriverLicenseVerificationStatus.Verified,
                SecurityStamp = Guid.NewGuid().ToString()
            }
        ];

        await _context.Users.AddRangeAsync(defaultUsers, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} default users for bookings.", defaultUsers.Count);
        return defaultUsers;
    }

    private static string? ResolveDataFilePath(string fileName)
    {
        string[] searchPaths =
        [
            Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeders", "Data", fileName),
            Path.Combine(AppContext.BaseDirectory, "Data", fileName),
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Persistence", "Seeders", "Data", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "CarRent.Infrastructure", "Persistence", "Seeders", "Data", fileName)
        ];

        foreach (string path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        string? currentDir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(currentDir))
        {
            string candidate = Path.Combine(currentDir, "Persistence", "Seeders", "Data", fileName);
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(currentDir, "src", "CarRent.Infrastructure", "Persistence", "Seeders", "Data", fileName);
            if (File.Exists(candidate)) return candidate;

            DirectoryInfo? parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }

        return null;
    }

    private sealed class BookingSeedDto
    {
        public int CarIndex { get; set; }
        public int UserIndex { get; set; }
        public double CreatedHoursAgo { get; set; }
        public int StartOffsetDays { get; set; }
        public int DurationDays { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public decimal? TotalPrice { get; set; }
    }
}
