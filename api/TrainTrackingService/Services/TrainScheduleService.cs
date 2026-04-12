using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;
using TrainTrackingService.Services.Interfaces;
using TrainTrackingService.Data;

namespace TrainTrackingService.Services
{
    public class TrainScheduleService : ITrainScheduleService
    {
        private readonly TrainTrackingContext _context;

        public TrainScheduleService(TrainTrackingContext context)
        {
            _context = context;
        }

        public async Task<TrainSchedule?> GetScheduleAsync(string trainId, DayOfWeek dayOfWeek)
        {
            return await _context.TrainSchedules
                .Include(s => s.Stops)
                .FirstOrDefaultAsync(s => s.TrainId == trainId && s.DayOfWeek == dayOfWeek);
        }

        public async Task<List<TrainSchedule>> GetDaySchedulesAsync(DayOfWeek dayOfWeek)
        {
            return await _context.TrainSchedules
                .Include(s => s.Stops)
                .Where(s => s.DayOfWeek == dayOfWeek)
                .ToListAsync();
        }

        public async Task<ScheduleStop?> GetNextStopAsync(string trainId, DayOfWeek dayOfWeek, TimeSpan currentTime)
        {
            var schedule = await GetScheduleAsync(trainId, dayOfWeek);
            return schedule?.Stops.FirstOrDefault(stop => stop.ArrivalTime > currentTime);
        }

        public async Task<List<ScheduleStop>> GetUpcomingStopsAsync(string trainId, DayOfWeek dayOfWeek, TimeSpan currentTime)
        {
            var schedule = await GetScheduleAsync(trainId, dayOfWeek);
            return schedule?.Stops.Where(stop => stop.ArrivalTime > currentTime).ToList() ?? new List<ScheduleStop>();
        }

        public async Task<NextArrivalResult?> GetNextArrivalEtaAsync(int? stopId, double? latitude, double? longitude)
        {
            // Find the stop either by ID or nearest to coordinates
            TrainStop? stop = null;
            if (stopId.HasValue)
            {
                stop = await _context.TrainStops.FirstOrDefaultAsync(s => s.Id == stopId.Value);
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                stop = await _context.TrainStops
                    .OrderBy(s => Math.Pow(s.Latitude - latitude.Value, 2) + Math.Pow(s.Longitude - longitude.Value, 2))
                    .FirstOrDefaultAsync();
            }
            if (stop == null) return null;

            var now = DateTime.UtcNow;
            var today = now.DayOfWeek;
            var schedules = await _context.TrainSchedules
                .Include(s => s.Stops)
                .Include(s => s.Route)
                .Where(s => s.DayOfWeek == today)
                .ToListAsync();

            NextArrivalResult? best = null;
            foreach (var schedule in schedules)
            {
                var nextStop = schedule.Stops
                    .Where(s => s.StopId == stop.Id && s.ArrivalTime > now.TimeOfDay)
                    .OrderBy(s => s.ArrivalTime)
                    .FirstOrDefault();
                if (nextStop != null)
                {
                    var eta = nextStop.ArrivalTime - now.TimeOfDay;
                    if (eta < TimeSpan.Zero) continue;
                    if (best == null || eta < best.ETA)
                    {
                        best = new NextArrivalResult
                        {
                            TrainId = schedule.TrainId,
                            RouteName = schedule.Route?.RouteName,
                            StationName = stop.StationName,
                            ArrivalTime = now.Date.Add(nextStop.ArrivalTime),
                            ETA = eta,
                            Latitude = stop.Latitude,
                            Longitude = stop.Longitude
                        };
                    }
                }
            }
            return best;
        }
    }
}
