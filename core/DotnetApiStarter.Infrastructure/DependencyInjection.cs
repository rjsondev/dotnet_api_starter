using DotnetApiStarter.Application;
using DotnetApiStarter.Application.Common.Behaviors;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetApiStarter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // # Register the database context to the service collection
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // # Register the IApplicationDbContext interface with the AppDbContext implementation
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // # Register all validators from the assembly containing the AssemblyMarker class
        services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

        // cfg (configuration object) - an instance of MediatRServiceConfiguration provided by the AddMediatR method.
        // It allows you to configure MediatR services, such as registering handlers, behaviors, and other components.
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
