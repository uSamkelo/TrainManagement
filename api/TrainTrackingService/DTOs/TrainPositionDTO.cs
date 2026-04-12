using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainTrackingService.Data.DTOs
{
    public class TrainPositionDTO
    {
        public string? TrainId { get; init; } = null;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Status { get; init; } = null;
        public string? ETA { get; init; } = null;
    }
}