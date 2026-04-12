using TrainTrackingService.Data;
using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Interfaces;
using TrainTrackingService.Models;
using Microsoft.EntityFrameworkCore;

namespace TrainTrackingService.Services
{
    public class TrainPositionService : ITrainPositionService
    {
        private readonly TrainTrackingContext _context;

        public TrainPositionService(TrainTrackingContext context)
        {
            _context = context;
        }

        public async Task<List<TrainPositionDTO>> GetAllAsync()
        {
            var positions = await _context.TrainPositions.ToListAsync();
            return positions.Select(p => new TrainPositionDTO
            {
                TrainId = p.TrainId,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Status = p.Status,
                ETA = p.ETA.ToString(@"hh\:mm")
            }).ToList();
        }

        public async Task<IEnumerable<TrainPosition>> GetByStationAsync(string station)
        {
            return await _context.TrainPositions.Where(tp => tp.TrainId == station).ToListAsync();
        }

        public async Task<IEnumerable<TrainPosition>> GetByLocationAsync(double latitude, double longitude)
        {
            var radius = 0.1;
            return await _context.TrainPositions
                .Where(tp => Math.Abs(tp.Latitude - latitude) < radius && Math.Abs(tp.Longitude - longitude) < radius)
                .ToListAsync();
        }

        public async Task CreateAsync(TrainPosition position)
        {
            _context.TrainPositions.Add(position);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var position = await _context.TrainPositions.FirstOrDefaultAsync(p => p.TrainId == id);
            if (position == null) return false;
            position.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateETAAsync(string id, TimeSpan eta)
        {
            var position = await _context.TrainPositions.FirstOrDefaultAsync(p => p.TrainId == id);
            if (position == null) return false;
            position.ETA = eta;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}