using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainTrackingService.Data.DTOs
{
    public class TrainPositionDTO
    {
        public string TrainId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }
        public string ETA { get; set; }
    }
}