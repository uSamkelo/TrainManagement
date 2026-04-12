using TrainTrackingService.Data;
using TrainTrackingService.Models;
using Microsoft.EntityFrameworkCore;
using TrainTrackingService.Interfaces;
using TrainTrackingService.Services.Interfaces;
using TrainTrackingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Enable controller-based endpoints
builder.Services.AddControllers();

// Add SignalR for real-time updates
builder.Services.AddSignalR();



// Add event model for train position
builder.Services.AddDbContext<TrainTrackingContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:TrainTrackingDb"]);
});

// Register event publisher
builder.Services.AddSingleton<ITrainPositionEventPublisher, TrainPublisher>();
builder.Services.AddSingleton<TrainSimulationService>();

// Register train simulation as a hosted service
builder.Services.AddHostedService<TrainSimulationHostedService>();

builder.Services.AddScoped<ITrainPositionService, TrainPositionService>();
builder.Services.AddScoped<ITrainScheduleService, TrainScheduleService>();
builder.Services.AddScoped<ITrainRouteService, TrainRouteService>();
builder.Services.AddScoped<IETACalculationService, ETACalculationService>();

var app = builder.Build();

// Ensure CORS is configured before authentication and authorization
app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200")); // Ensure this is correctly set to your frontend origin

// Seed initial train data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TrainTrackingContext>();
    try
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed routes, stops, and schedules
        await TrainDataSeeder.SeedAsync(context);

        // Check if trains already exist
        if (!context.TrainPositions.Any(t => t.TrainId == "SouthernLine_T01"))
        {
            context.TrainPositions.Add(new TrainPosition
            {
                TrainId = "SouthernLine_T01",
                Latitude = -33.9234,
                Longitude = 18.4262,
                Status = "in-transit",
                ETA = TimeSpan.Zero,
                Timestamp = DateTime.UtcNow,
                CurrentStop = "Cape Town Station",
                NextStop = "",
                NextStopETA = TimeSpan.Zero
            });
        }

        if (!context.TrainPositions.Any(t => t.TrainId == "NorthernLine_T02"))
        {
            context.TrainPositions.Add(new TrainPosition
            {
                TrainId = "NorthernLine_T02",
                Latitude = -33.9234,
                Longitude = 18.4262,
                Status = "in-transit",
                ETA = TimeSpan.Zero,
                Timestamp = DateTime.UtcNow,
                CurrentStop = "Cape Town Station",
                NextStop = "",
                NextStopETA = TimeSpan.Zero
            });
        }

        if (!context.TrainPositions.Any(t => t.TrainId == "CapeFlatsLine_T03"))
        {
            context.TrainPositions.Add(new TrainPosition
            {
                TrainId = "CapeFlatsLine_T03",
                Latitude = -33.9234,
                Longitude = 18.4262,
                Status = "in-transit",
                ETA = TimeSpan.Zero,
                Timestamp = DateTime.UtcNow,
                CurrentStop = "Cape Town Station",
                NextStop = "",
                NextStopETA = TimeSpan.Zero
            });
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

// Map SignalR hub for real-time train updates
app.MapHub<TrainUpdateHub>("/train-update");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
