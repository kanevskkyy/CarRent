using CarRent.Application.Interfaces.Services;
using CarRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Testcontainers.MsSql;
using Xunit;

namespace CarRent.Tests.Integration;

public sealed class SqlServerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly Dictionary<string, string?> _previousEnvironment = new();
    public ConcurrentQueue<string> CapturedLogs { get; } = new();
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithDatabase("CarRentIntegrationTests")
        .WithPassword("CarRent!Integration2026_Strong")
        .Build();

    public HttpClient Client { get; private set; } = null!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _previousEnvironment["JwtSettings__SecretKey"] = Environment.GetEnvironmentVariable("JwtSettings__SecretKey");
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "integration-test-secret-key-at-least-32-characters-long");
        _previousEnvironment["JwtSettings__Issuer"] = Environment.GetEnvironmentVariable("JwtSettings__Issuer");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "CarRent.IntegrationTests");
        _previousEnvironment["JwtSettings__Audience"] = Environment.GetEnvironmentVariable("JwtSettings__Audience");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "CarRent.IntegrationTests");
        await _sqlServer.StartAsync();
        Client = CreateClient();

        using IServiceScope scope = Services.CreateScope();
        CarRentDbContext db = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        await db.Database.EnsureCreatedAsync();

        RoleManager<IdentityRole<Guid>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (string role in new[] { "User", "Admin" })
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;
            IdentityResult result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create test role {role}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Client?.Dispose();
        await base.DisposeAsync();
        await _sqlServer.DisposeAsync();
        foreach ((string key, string? value) in _previousEnvironment)
            Environment.SetEnvironmentVariable(key, value);
    }

    private sealed class QueueLoggerProvider(ConcurrentQueue<string> logs) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new QueueLogger(logs, categoryName);
        public void Dispose() { }
    }

    private sealed class QueueLogger(ConcurrentQueue<string> logs, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            logs.Enqueue($"{categoryName}: {formatter(state, exception)}");
            if (exception is not null)
                logs.Enqueue(exception.ToString());
        }
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.AddProvider(new QueueLoggerProvider(CapturedLogs)));
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "integration-test-secret-key-at-least-32-characters-long",
                ["JwtSettings:Issuer"] = "CarRent.IntegrationTests",
                ["JwtSettings:Audience"] = "CarRent.IntegrationTests"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbSeeder>();
            services.RemoveAll<DbContextOptions<CarRentDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CarRentDbContext>>();
            services.AddDbContext<CarRentDbContext>(options => options.UseSqlServer(_sqlServer.GetConnectionString()));
        });
    }
}

