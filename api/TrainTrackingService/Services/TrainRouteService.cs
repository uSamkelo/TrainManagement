using Microsoft.EntityFrameworkCore;
using TrainTrackingService.Data;
using TrainTrackingService.Models;
using TrainTrackingService.Services.Interfaces;

namespace TrainTrackingService.Services
{
    public class TrainRouteService : ITrainRouteService
    {
        private readonly TrainTrackingContext _context;

        public TrainRouteService(TrainTrackingContext context)
        {
            _context = context;
        }

        public async Task<TrainRoute?> GetRouteAsync(string routeId)
        {
            return await _context.TrainRoutes
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .FirstOrDefaultAsync(r => r.Id == routeId);
        }

        public async Task<List<TrainRoute>> GetAllRoutesAsync()
        {
            return await _context.TrainRoutes
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .ToListAsync();
        }

        public async Task<TrainRoute?> GetRouteByTrainIdAsync(string trainId)
        {
            // Train IDs follow convention: RouteName_TXX (e.g., SouthernLine_T01)
            var routeId = trainId.Contains('_') ? trainId[..trainId.LastIndexOf('_')] : trainId;
            return await GetRouteAsync(routeId);
        }

        public async Task<TrainStop?> GetNearestStopAsync(double latitude, double longitude, double radiusKm = 1.0)
        {
            var stops = await _context.TrainStops.ToListAsync();
            return stops
                .Select(s => new { Stop = s, Distance = HaversineKm(latitude, longitude, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .FirstOrDefault()?.Stop;
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
