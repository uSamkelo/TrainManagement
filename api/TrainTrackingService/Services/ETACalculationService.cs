using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;
using TrainTrackingService.Services.Interfaces;

namespace TrainTrackingService.Services
{
    public class ETACalculationService : IETACalculationService
    {
        private readonly ITrainRouteService _routeService;
        private readonly ITrainScheduleService _scheduleService;

        public ETACalculationService(ITrainRouteService routeService, ITrainScheduleService scheduleService)
        {
            _routeService = routeService;
            _scheduleService = scheduleService;
        }

        public async Task<TrainWithETADto?> CalculateETAAsync(TrainPosition position, string? selectedStation = null)
        {
            if (string.IsNullOrEmpty(position.TrainId)) return null;

            var route = await _routeService.GetRouteByTrainIdAsync(position.TrainId);
            if (route == null) return null;

            var now = DateTime.UtcNow;
            var upcomingStops = await _scheduleService.GetUpcomingStopsAsync(
                position.TrainId, now.DayOfWeek, now.TimeOfDay);

            var dto = new TrainWithETADto
            {
                TrainId = position.TrainId,
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                Status = position.Status ?? "unknown",
                CurrentStop = position.CurrentStop,
                NextStop = position.NextStop,
                UpcomingStops = upcomingStops.Select(s =>
                {
                    var stop = route.Stops.FirstOrDefault(rs => rs.Id == s.StopId);
                    return new UpcomingStopDto
                    {
                        StationName = stop?.StationName ?? $"Stop #{s.StopId}",
                        ArrivalTime = now.Date.Add(s.ArrivalTime),
                        ETA = s.ArrivalTime - now.TimeOfDay
                    };
                }).ToList()
            };

            if (upcomingStops.Count != 0)
            {
                var nextStop = upcomingStops[0];
                dto.NextStopArrivalTime = now.Date.Add(nextStop.ArrivalTime);
                dto.ETAToNextStop = nextStop.ArrivalTime - now.TimeOfDay;
            }

            if (!string.IsNullOrEmpty(selectedStation))
            {
                var targetScheduleStop = upcomingStops.FirstOrDefault(s =>
                    route.Stops.Any(rs => rs.StationName == selectedStation && rs.Id == s.StopId));
                if (targetScheduleStop != null)
                {
                    dto.SelectedStationArrivalTime = now.Date.Add(targetScheduleStop.ArrivalTime);
                    dto.ETAToSelectedStation = targetScheduleStop.ArrivalTime - now.TimeOfDay;
                }
            }

            return dto;
        }

        public async Task<TimeSpan?> GetETAToStopAsync(
            string trainId, TrainStop targetStop,
            double currentLatitude, double currentLongitude)
        {
            var now = DateTime.UtcNow;
            var upcomingStops = await _scheduleService.GetUpcomingStopsAsync(
                trainId, now.DayOfWeek, now.TimeOfDay);

            var match = upcomingStops.FirstOrDefault(s => s.StopId == targetStop.Id);
            if (match == null) return null;

            return match.ArrivalTime - now.TimeOfDay;
        }
    }
}
