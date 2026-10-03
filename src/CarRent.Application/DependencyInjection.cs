using System.Reflection;
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
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        return services;
    }
}