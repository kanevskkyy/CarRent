using System.Reflection;
using CarRent.Application.Common.Behaviors;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        Assembly applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddAutoMapper(cfg => 
            cfg.AddMaps(applicationAssembly));
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));;
        });
        
        services.AddValidatorsFromAssembly(applicationAssembly);
        
        services.AddScoped<IPaymentConfirmationService, PaymentConfirmationService>();

        return services;
    }
}