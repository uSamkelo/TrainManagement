using TrainTrackingService.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using MassTransit;
using RabbitMQ;
using Microsoft.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Enable controller-based endpoints
builder.Services.AddControllers();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:HostName"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});



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

var app = builder.Build();

// Ensure CORS is configured before authentication and authorization
app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200")); // Ensure this is correctly set to your frontend origin


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

// Map SignalR hub for real-time train updates
app.MapHub<TrainUpdateHub>("/trainhub");

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
