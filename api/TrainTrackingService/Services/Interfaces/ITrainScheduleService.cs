using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;

namespace TrainTrackingService.Services.Interfaces
{
    public interface ITrainScheduleService
    {
        Task<TrainSchedule?> GetScheduleAsync(string trainId, DayOfWeek dayOfWeek);
        Task<List<TrainSchedule>> GetDaySchedulesAsync(DayOfWeek dayOfWeek);
        Task<ScheduleStop?> GetNextStopAsync(string trainId, DayOfWeek dayOfWeek, TimeSpan currentTime);
        Task<List<ScheduleStop>> GetUpcomingStopsAsync(string trainId, DayOfWeek dayOfWeek, TimeSpan currentTime);
        Task<NextArrivalResult?> GetNextArrivalEtaAsync(int? stopId, double? latitude, double? longitude);
    }
}
