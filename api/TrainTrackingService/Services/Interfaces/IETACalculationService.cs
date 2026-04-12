using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;

namespace TrainTrackingService.Services.Interfaces
{
    /// <summary>
    /// Service for calculating ETAs based on schedule and current position.
    /// Single Responsibility: ETA calculation logic
    /// </summary>
    public interface IETACalculationService
    {
        Task<TrainWithETADto?> CalculateETAAsync(
            TrainPosition position,
            string? selectedStation = null);
        
        Task<TimeSpan?> GetETAToStopAsync(
            string trainId,
            TrainStop targetStop,
            double currentLatitude,
            double currentLongitude);
    }
}
