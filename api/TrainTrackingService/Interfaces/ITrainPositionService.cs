using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;

namespace TrainTrackingService.Interfaces
{
    public interface ITrainPositionService
    {
        Task<List<TrainPositionDTO>> GetAllAsync();
        Task<IEnumerable<TrainPosition>> GetByStationAsync(string station);
        Task<IEnumerable<TrainPosition>> GetByLocationAsync(double latitude, double longitude);
        Task CreateAsync(TrainPosition position);
        Task<bool> UpdateStatusAsync(string id, string status);
        Task<bool> UpdateETAAsync(string id, TimeSpan eta);
    }
}