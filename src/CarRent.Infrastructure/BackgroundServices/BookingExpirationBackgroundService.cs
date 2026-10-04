using CarRent.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.BackgroundServices;

public class BookingExpirationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingExpirationBackgroundService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ExpirationTimeout = TimeSpan.FromMinutes(10);

    public BookingExpirationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingExpirationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Expiration Background Service is starting.");

        using PeriodicTimer timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing expired bookings.");
            }
        }

        _logger.LogInformation("Booking Expiration Background Service is stopping.");
    }

    private async Task ProcessExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        IBookingRepository bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        int cancelledCount = await bookingRepository.CancelExpiredPendingBookingsAsync(ExpirationTimeout, cancellationToken);

        if (cancelledCount > 0)
        {
            _logger.LogInformation("Cancelled {Count} expired pending booking(s).", cancelledCount);
        }
    }
}