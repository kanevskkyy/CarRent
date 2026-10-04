namespace CarRent.Application.Interfaces.Services;

public interface IDbSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}