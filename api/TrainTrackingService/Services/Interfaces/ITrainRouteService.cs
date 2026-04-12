using TrainTrackingService.Models;

namespace TrainTrackingService.Services.Interfaces
{
    /// <summary>
    /// Service for managing train routes and their stops.
    /// Single Responsibility: Managing route data
    /// </summary>
    public interface ITrainRouteService
    {
        Task<TrainRoute?> GetRouteAsync(string routeId);
        Task<List<TrainRoute>> GetAllRoutesAsync();
        Task<TrainRoute?> GetRouteByTrainIdAsync(string trainId);
        Task<TrainStop?> GetNearestStopAsync(double latitude, double longitude, double radiusKm = 1.0);
    }
}
