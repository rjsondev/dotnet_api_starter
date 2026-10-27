using DotnetApiStarter.Application;
using DotnetApiStarter.Application.Common.Behaviors;
using DotnetApiStarter.Application.Common.Interfaces;
using DotnetApiStarter.Infrastructure.Authentication;
using DotnetApiStarter.Infrastructure.Caching;
using DotnetApiStarter.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        // # Register the Redis cache service using StackExchange.Redis
        services.AddStackExchangeRedisCache(options => { options.Configuration = configuration.GetConnectionString("Redis"); options.InstanceName = "dotnetapistarter:"; });
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            // # Retrieve the Redis connection string from configuration
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is missing.");

            // # Parse the string and force abortConnect to false
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 2000;
            options.SyncTimeout = 1000;         // Cut data operation wait to 1 second
            options.ConnectRetry = 1;           // Only retry twice before failing

            // # Connect and return the shared singleton instance
            return ConnectionMultiplexer.Connect(options);
        });

        // # Register the IApplicationDbContext interface with the AppDbContext implementation
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // # Register the JwtTokenService for generating JWT tokens
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // # Register the BCryptPasswordHasher for password hashing and verification
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

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
