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

namespace CarRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
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
        
        //USER IDENTITY
        services.AddHttpContextAccessor();
        services.AddScoped<IUserClaimsService, UserIdentityClaimsService>();

        //SEEDERS
        services.AddScoped<IDbSeeder, UserSeeder>();
        services.AddScoped<IDbSeeder, CarSeeder>();
        services.AddScoped<IDbSeeder, BookingSeeder>();
        
        //BACKGROUND SERVICE
        services.AddHostedService<BookingExpirationBackgroundService>();
        
        return services;
    }
}