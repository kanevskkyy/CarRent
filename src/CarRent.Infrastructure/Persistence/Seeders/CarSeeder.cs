using System.Text.Json;
using System.Text.Json.Serialization;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Persistence.Seeders;

public class CarSeeder : IDbSeeder
{
    private readonly CarRentDbContext _context;
    private readonly ILogger<CarSeeder> _logger;

    public CarSeeder(CarRentDbContext context, ILogger<CarSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Cars.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Cars table already contains records. Skipping car seeding.");
            return;
        }

        string? filePath = ResolveDataFilePath("cars.json");
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("cars.json seed file was not found. Looked in AppContext.BaseDirectory and parent directories.");
            return;
        }

        _logger.LogInformation("Seeding cars from {FilePath}...", filePath);
        string json = await File.ReadAllTextAsync(filePath, cancellationToken);

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        List<Car>? cars = JsonSerializer.Deserialize<List<Car>>(json, options);
        if (cars == null || cars.Count == 0)
        {
            _logger.LogWarning("No cars found in {FilePath} to seed.", filePath);
            return;
        }

        await _context.Cars.AddRangeAsync(cars, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} cars.", cars.Count);
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
}
