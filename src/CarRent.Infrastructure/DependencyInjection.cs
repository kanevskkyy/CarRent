using CarRent.Application.Common.Settings;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Services;
using CarRent.Infrastructure.BackgroundServices;
using CarRent.Infrastructure.ExternalServices;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Persistence.Repositories;
using CarRent.Infrastructure.Persistence.Seeders;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CarRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHttpClient();
        
        //DB CONTEXT
        services.AddDbContext<CarRentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        //REPOSITORY
        services.AddScoped<ICarRepository, EfCarRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
        
        //UOW
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        //CLOUDINARY
        services.Configure<CloudinarySettings>(
            configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<IImageUploaderService, CloudinaryImageUploaderService>();
        
        //JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        
        //USER IDENTITY
        services.AddHttpContextAccessor();
        services.AddScoped<IUserClaimsService, UserIdentityClaimsService>();

        //SEEDERS
        services.AddScoped<IDbSeeder, UserSeeder>();
        services.AddScoped<IDbSeeder, CarSeeder>();
        services.AddScoped<IDbSeeder, BookingSeeder>();
        
        //BACKGROUND SERVICE
        services.AddHostedService<BookingExpirationBackgroundService>();
        
        //EXTERNAL AUTH
        services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.SectionName));
        services.AddScoped<IExternalAuthProvider, GoogleAuthProvider>();
        services.Configure<GitHubSettings>(configuration.GetSection(GitHubSettings.SectionName));
        services.AddScoped<IExternalAuthProvider, GitHubAuthProvider>();
        
        //PAYPAL
        services.Configure<PayPalSettings>(configuration.GetSection(PayPalSettings.SectionName));
        services.AddHttpClient<IPaymentGateway, PayPalPaymentGateway>();
        
        //REDIS
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(
                configuration.GetConnectionString("Redis") ?? "localhost:6379");
            options.AbortOnConnectFail = false; 

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddScoped<IFavoriteCarStore, RedisFavoriteCarStore>();
        
        return services;
    }
}