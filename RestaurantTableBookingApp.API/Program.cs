using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantTableBookingApp.Data;
using RestaurantTableBookingApp.Service;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using LSC.RestaurantTableBookingApp.API;

namespace RestaurantTableBookingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set the environment variable programmatically
            Environment.SetEnvironmentVariable("DIAGNOSTICS_AZUREBLOBCONTAINERSASURL", "YourSASUrlHere");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateBootstrapLogger(); // Using CreateBootstrapLogger for early initialization

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                var configuration = builder.Configuration;

                builder.Services.AddApplicationInsightsTelemetry(configuration);

                builder.Host.UseSerilog((context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.ApplicationInsights(
                            services.GetRequiredService<TelemetryConfiguration>(),
                            TelemetryConverter.Traces)
                );

                builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
                builder.Services.AddScoped<IRestaurantService, RestaurantService>();

                builder.Services.AddDbContext<RestaurantTableBookingDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DbContext") ?? "")
                        .EnableSensitiveDataLogging()
                );

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        Log.Error(exception, "Unhandled exception occurred. {ExceptionDetails}", exception?.ToString());

                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
                    });
                });

                app.UseMiddleware<RequestResponseLoggingMiddleware>();

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start correctly");
            }
            finally
            {
                Log.Information("Application is shutting down...");
                Log.CloseAndFlush();
            }
        }
    }
}
