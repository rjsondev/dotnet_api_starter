using DotnetApiStarter.Api.Extensions;
using DotnetApiStarter.Api.Features.Authentication;
using DotnetApiStarter.Api.Features.Products;
using DotnetApiStarter.Api.Middleware;
using DotnetApiStarter.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting DotnetApiStarter API");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // # Serilog becomes the provider behind ILogger<T>.
    builder.Services.AddSerilog((services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // # Dependency Injection for Infrastructure Layer
    builder.Services.AddInfrastructure(builder.Configuration);

    // # Register commands and queries with MediatR
    builder.Services.AddApplication();

    // # Register the global exception handler and problem details middleware
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // # Register JWT authentication and authorization
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // # Register rate limiting policies
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("LoginPolicy", limiter =>
        {
            limiter.PermitLimit = 5;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
            limiter.AutoReplenishment = true;
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    // var summaries = new[]
    // {
    //     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    // };

    // app.MapGet("/weatherforecast", () =>
    // {
    //     var forecast =  Enumerable.Range(1, 5).Select(index =>
    //         new WeatherForecast
    //         (
    //             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //             Random.Shared.Next(-20, 55),
    //             summaries[Random.Shared.Next(summaries.Length)]
    //         ))
    //         .ToArray();
    //     return forecast;
    // })
    // .WithName("GetWeatherForecast");

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.Run();

    // record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    // {
    //     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    // }
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shutting down DotnetApiStarter API");
    Log.CloseAndFlush();
}