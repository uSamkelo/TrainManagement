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
    private readonly ILogger<TrainSimulationService> _logger;
    private readonly IHubContext<TrainUpdateHub> _hubContext;
    private double _speedKmh = 100; // km/h
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(2); // Update frequency
    private readonly TimeSpan _dbSaveInterval = TimeSpan.FromSeconds(5); // How often to save to DB
    private DateTime _lastDbSave = DateTime.UtcNow;

    // Define train routes with stops (latitude, longitude, stop name, cumulative distance in km)
    private readonly Dictionary<string, List<(double lat, double lon, string stopName, double distanceKm)>> _trainRoutes = new()
{
    {
        "SouthernLine_T01", new List<(double, double, string, double)>
        {
            (-33.9234, 18.4262, "Cape Town Station", 0.0),
            (-33.9284, 18.4414, "Woodstock", 1.8),
            (-33.9356, 18.4619, "Salt River", 3.7),
            (-33.9458, 18.4719, "Observatory", 5.1),
            (-33.9634, 18.4717, "Newlands", 8.8),
            (-33.9748, 18.4712, "Claremont", 10.4),
            (-34.0041, 18.4646, "Wynberg", 14.2),
            (-34.0536, 18.4529, "Retreat", 20.2),
            (-34.1086, 18.4690, "Muizenberg", 26.5),
            (-34.1281, 18.4503, "Kalk Bay", 29.2),
            (-34.1414, 18.4326, "Fish Hoek", 31.3),
            (-34.1868, 18.4277, "Simon's Town", 37.1),
            // Return Journey
            (-34.1414, 18.4326, "Fish Hoek", 42.9),
            (-34.1086, 18.4690, "Muizenberg", 47.7),
            (-34.0041, 18.4646, "Wynberg", 60.0),
            (-33.9234, 18.4262, "Cape Town Station", 74.2)
        }
    },
    {
        "NorthernLine_T02", new List<(double, double, string, double)>
        {
            (-33.9234, 18.4262, "Cape Town Station", 0.0),
            (-33.9356, 18.4619, "Salt River", 3.7),
            (-33.9254, 18.4878, "Maitland", 6.2),
            (-33.9048, 18.5304, "Goodwood", 12.1),
            (-33.9015, 18.5445, "Vasco", 13.6),
            (-33.9045, 18.5601, "Parow", 15.3),
            (-33.9019, 18.6300, "Bellville", 22.1),
            (-33.8654, 18.7062, "Kraaifontein", 31.4),
            // Return Journey
            (-33.9019, 18.6300, "Bellville", 40.7),
            (-33.9234, 18.4262, "Cape Town Station", 62.8)
        }
    },
    {
        "CapeFlatsLine_T03", new List<(double, double, string, double)>
        {
            (-33.9234, 18.4262, "Cape Town Station", 0.0),
            (-33.9284, 18.4414, "Woodstock", 1.8),
            (-33.9356, 18.4619, "Salt River", 3.7),
            (-33.9310, 18.4780, "Koeberg Road", 5.2),
            (-33.9254, 18.4878, "Maitland", 6.2),
            (-33.9420, 18.5310, "Langa", 9.8),
            (-33.9510, 18.5520, "Bonteheuwel", 11.5),
            (-33.9630, 18.5050, "Athlone", 13.2),
            (-33.9710, 18.5590, "Heideveld", 15.0),
            (-33.9880, 18.5730, "Nyanga", 17.2),
            (-34.0100, 18.5800, "Philippi", 19.8),
            (-34.0450, 18.6160, "Mitchells Plain", 25.3),
            (-34.0390, 18.6770, "Khayelitsha", 30.8),
            // Return Journey
            (-34.0450, 18.6160, "Mitchells Plain", 36.3),
            (-33.9630, 18.5050, "Athlone", 48.1),
            (-33.9234, 18.4262, "Cape Town Station", 61.6)
        }
    }
};

    // Track train progress: trainId -> cumulative distance traveled (km)
    private readonly Dictionary<string, double> _trainDistanceTraveled = new()
    {
        { "SouthernLine_T01", 0.0 },
        { "NorthernLine_T02", 0.0 },
        { "CapeFlatsLine_T03", 0.0 }
    };

    // Track total route distances
    private readonly Dictionary<string, double> _routeTotalDistances = new()
    {
        { "SouthernLine_T01", 74.2 },
        { "NorthernLine_T02", 62.8 },
        { "CapeFlatsLine_T03", 61.6 }
    };

    // Track stop times: trainId -> when the train started stopping at current location
    private readonly Dictionary<string, DateTime> _trainStopTimes = new()
    {
        { "SouthernLine_T01", DateTime.MinValue },
        { "NorthernLine_T02", DateTime.MinValue },
        { "CapeFlatsLine_T03", DateTime.MinValue }
    };

    // Stop duration in seconds
    private const int StopDurationSeconds = 30;

    public TrainSimulationService(IServiceProvider serviceProvider, ILogger<TrainSimulationService> logger, IHubContext<TrainUpdateHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    public void DefineRoute(List<(string StationName, double Distance)> route)
    {
        // This method is kept for compatibility but not used in linear simulation
    }

    public Dictionary<string, List<(double lat, double lon, string stopName, double distanceKm)>> GetTrainRoutes()
    {
        return _trainRoutes;
    }

    public Dictionary<string, double> GetRouteTotalDistances()
    {
        return _routeTotalDistances;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Train simulation service started - Linear movement with database updates");
        
        while (true)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TrainTrackingContext>();

                    // Get all trains from database
                    var dbTrains = context.TrainPositions.ToList();

                    foreach (var dbTrain in dbTrains)
                    {
                        if (string.IsNullOrEmpty(dbTrain.TrainId))
                            continue;

                        // Update train position based on linear movement
                        UpdateTrainPosition(dbTrain);

                        // Send real-time update via SignalR
                        await SendTrainUpdateAsync(dbTrain);
                    }

                    // Save to database periodically
                    if ((DateTime.UtcNow - _lastDbSave) >= _dbSaveInterval)
                    {
                        await context.SaveChangesAsync();
                        _lastDbSave = DateTime.UtcNow;
                        _logger.LogInformation("Train positions saved to database");
                    }
                }

                await Task.Delay(_updateInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in train simulation");
            }
        }
    }

    private void UpdateTrainPosition(TrainPosition position)
    {
        if (string.IsNullOrEmpty(position.TrainId) || !_trainRoutes.ContainsKey(position.TrainId))
            return;

        var route = _trainRoutes[position.TrainId];
        double currentDistance = _trainDistanceTraveled[position.TrainId];
        double totalDistance = _routeTotalDistances[position.TrainId];

        // Check if train is at a stop
        var (atStop, stopName) = IsTrainAtStop(route, currentDistance);
        
        if (atStop && stopName != null && _trainStopTimes[position.TrainId] == DateTime.MinValue)
        {
            // Just arrived at stop - start the timer
            _trainStopTimes[position.TrainId] = DateTime.UtcNow;
            position.Status = "stopped";
        }
        else if (atStop && stopName != null && _trainStopTimes[position.TrainId] != DateTime.MinValue)
        {
            // Train is stopped at a station
            var stopElapsed = (DateTime.UtcNow - _trainStopTimes[position.TrainId]).TotalSeconds;
            if (stopElapsed < StopDurationSeconds)
            {
                // Still stopping
                position.Status = "stopped";
            }
            else
            {
                // Stop duration complete - clear the stop time and move the train
                _trainStopTimes[position.TrainId] = DateTime.MinValue;
                
                // Now move the train forward
                double distanceInThisUpdate = (_speedKmh / 3600.0) * _updateInterval.TotalSeconds;
                currentDistance += distanceInThisUpdate;

                // Loop back to start if reached the end
                if (currentDistance >= totalDistance)
                {
                    currentDistance = 0.0;
                }

                // Update distance traveled
                _trainDistanceTraveled[position.TrainId] = currentDistance;
                position.Status = "in-transit";
            }
        }
        else if (!atStop)
        {
            // Train is not at a stop, move it
            _trainStopTimes[position.TrainId] = DateTime.MinValue;

            // Calculate distance traveled in this update interval
            double distanceInThisUpdate = (_speedKmh / 3600.0) * _updateInterval.TotalSeconds; // km
            
            currentDistance += distanceInThisUpdate;

            // Loop back to start if reached the end
            if (currentDistance >= totalDistance)
            {
                currentDistance = 0.0;
            }

            // Update distance traveled
            _trainDistanceTraveled[position.TrainId] = currentDistance;
            position.Status = "in-transit";
        }

        // Update position coordinates and stop info
        currentDistance = _trainDistanceTraveled[position.TrainId];
        var (newLat, newLon, currentStop, nextStop, nextStopDistance) = InterpolatePosition(route, currentDistance);
        
        position.Latitude = newLat;
        position.Longitude = newLon;
        position.Timestamp = DateTime.UtcNow;
        position.CurrentStop = currentStop;
        position.NextStop = nextStop;

        // Calculate remaining distance and ETA
        double remainingDistance = totalDistance - currentDistance;
        if (remainingDistance < 0) remainingDistance = 0;
        double etaSeconds = remainingDistance > 0 ? (remainingDistance / _speedKmh) * 3600 : 0;
        position.ETA = TimeSpan.FromSeconds(etaSeconds);

        // Calculate ETA to next stop
        if (!string.IsNullOrEmpty(nextStop) && nextStopDistance > 0)
        {
            double nextStopEtaSeconds = (nextStopDistance / _speedKmh) * 3600;
            position.NextStopETA = TimeSpan.FromSeconds(nextStopEtaSeconds);
        }
        else
        {
            position.NextStopETA = null;
        }

        // Update status based on ETA
        if (position.Status != "stopped")
        {
            position.Status = DetermineStatus(position.ETA);
        }
    }

    private (bool atStop, string? stopName) IsTrainAtStop(
        List<(double lat, double lon, string stopName, double distanceKm)> route,
        double currentDistance)
    {
        const double stopThreshold = 0.1; // km - considered "at" a stop if within this distance
        
        foreach (var stop in route)
        {
            if (Math.Abs(currentDistance - stop.distanceKm) <= stopThreshold)
            {
                return (true, stop.stopName);
            }
        }
        
        return (false, null);
    }

    private (double lat, double lon, string currentStop, string nextStop, double nextStopDistance) InterpolatePosition(
        List<(double lat, double lon, string stopName, double distanceKm)> route, 
        double targetDistance)
    {
        const double stopThreshold = 0.1; // km - considered "at" a stop if within this distance

        // Check if we're at or very near a stop
        foreach (var stop in route)
        {
            if (Math.Abs(targetDistance - stop.distanceKm) <= stopThreshold)
            {
                // Train is at or very near a stop
                // Find next stop distance
                double nextStopDist = 0;
                var nextStopIndex = route.FindIndex(s => s.stopName == stop.stopName) + 1;
                if (nextStopIndex < route.Count)
                {
                    nextStopDist = route[nextStopIndex].distanceKm - targetDistance;
                }
                return (stop.lat, stop.lon, stop.stopName, "", nextStopDist);
            }
        }

        // Find the two waypoints we're between
        for (int i = 0; i < route.Count - 1; i++)
        {
            var current = route[i];
            var next = route[i + 1];

            if (targetDistance > current.distanceKm && targetDistance < next.distanceKm)
            {
                // Linear interpolation between two points
                double segmentDistance = next.distanceKm - current.distanceKm;
                double distanceInSegment = targetDistance - current.distanceKm;
                double fraction = segmentDistance > 0 ? distanceInSegment / segmentDistance : 0;

                double interpolatedLat = current.lat + (next.lat - current.lat) * fraction;
                double interpolatedLon = current.lon + (next.lon - current.lon) * fraction;

                // En-route: current stop is empty, next stop is the destination
                // Distance to next stop
                double nextStopDist = next.distanceKm - targetDistance;
                return (interpolatedLat, interpolatedLon, "", next.stopName, nextStopDist);
            }
        }

        // If we've reached the end, return the last waypoint as current stop
        var lastStop = route[route.Count - 1];
        return (lastStop.lat, lastStop.lon, lastStop.stopName, "", 0);
    }

    public TimeSpan CalculateETA()
    {
        // This method is kept for compatibility
        return TimeSpan.Zero;
    }

    private string DetermineStatus(TimeSpan eta)
    {
        if (eta.TotalMinutes <= 0)
        {
            return "arrived";
        }
        else if (eta.TotalMinutes <= 2)
        {
            return "arriving";
        }
        else if (eta.TotalMinutes <= 5)
        {
            return "on-time";
        }
        else
        {
            return "in-transit";
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
                eta = position.ETA.ToString(@"hh\:mm\:ss"),
                nextStopEta = position.NextStopETA?.ToString(@"hh\:mm\:ss") ?? "",
                timestamp = position.Timestamp,
                currentStop = position.CurrentStop,
                nextStop = position.NextStop,
                trail = new[] { new[] { position.Latitude, position.Longitude } }
            };

            await _hubContext.Clients.All.SendAsync("ReceiveTrainUpdate", update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending train update via SignalR");
        }
    }
}
