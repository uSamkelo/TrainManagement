using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrainTrackingService.Data;
using TrainTrackingService.Models;

public class TrainSimulationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TrainUpdateHub> _hubContext;
    private readonly ILogger<TrainSimulationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private Random _random = new Random();

    // Define train routes (waypoints)
    private readonly Dictionary<string, List<(double, double)>> _routes = new()
    {
        {
            "Train123", new List<(double, double)>
            {
                (40.7128, -74.0060),  // New York starting
                (40.7150, -74.0050),
                (40.7180, -74.0040),
                (40.7200, -74.0050),
                (40.7220, -74.0060),
            }
        },
        {
            "Train124", new List<(double, double)>
            {
                (40.7200, -74.0100),  // New York starting (different location)
                (40.7180, -74.0090),
                (40.7160, -74.0100),
                (40.7140, -74.0110),
                (40.7120, -74.0100),
            }
        }
    };

    // Track position index for each train
    private readonly Dictionary<string, int> _trainPositionIndex = new()
    {
        { "Train123", 0 },
        { "Train124", 0 }
    };

    public TrainSimulationService(IServiceProvider serviceProvider, IHubContext<TrainUpdateHub> hubContext, ILogger<TrainSimulationService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Train simulation service started");
        
        while (true)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TrainTrackingContext>();

                    // Simulate train movement
                    foreach (var position in context.TrainPositions.ToList())
                    {
                        UpdateTrainPosition(position);
                        await context.SaveChangesAsync();

                        // Send real-time update via SignalR
                        await SendTrainUpdateAsync(position);
                    }
                }

                await Task.Delay(_interval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in train simulation");
            }
        }
    }

    private void UpdateTrainPosition(TrainPosition position)
    {
        // Get the route for this train
        if (!_routes.ContainsKey(position.TrainId))
        {
            return;
        }

        var route = _routes[position.TrainId];
        var currentIndex = _trainPositionIndex[position.TrainId];

        // Move to next waypoint
        if (currentIndex < route.Count)
        {
            var (targetLat, targetLon) = route[currentIndex];
            
            // Add small random variations for realistic movement
            var latVariation = (_random.NextDouble() - 0.5) * 0.0002;
            var lonVariation = (_random.NextDouble() - 0.5) * 0.0002;

            position.Latitude = targetLat + latVariation;
            position.Longitude = targetLon + lonVariation;
            position.Timestamp = DateTime.UtcNow;

            // Move to next waypoint every 3 iterations
            if (currentIndex % 3 == 0 && currentIndex < route.Count - 1)
            {
                _trainPositionIndex[position.TrainId]++;
            }
        }

        // Update ETA (decrease by interval)
        position.ETA = position.ETA.Subtract(_interval);
        if (position.ETA < TimeSpan.Zero)
        {
            position.ETA = TimeSpan.Zero;
        }

        // Update status based on ETA
        position.Status = DetermineStatus(position.ETA);
    }

    private string DetermineStatus(TimeSpan eta)
    {
        if (eta.TotalMinutes <= 0)
        {
            return "arrived";
        }
        else if (eta.TotalMinutes <= 5)
        {
            return "on-time";
        }
        else if (eta.TotalMinutes <= 15)
        {
            return "in-transit";
        }
        else
        {
            return "delayed";
        }
    }

    private async Task SendTrainUpdateAsync(TrainPosition position)
    {
        try
        {
            var update = new
            {
                trainId = position.TrainId,
                latitude = position.Latitude,
                longitude = position.Longitude,
                status = position.Status,
                eta = position.ETA.ToString(@"hh\:mm"),
                timestamp = position.Timestamp
            };

            await _hubContext.Clients.All.SendAsync("ReceiveTrainUpdate", update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending train update via SignalR");
        }
    }
}
