namespace TrainTrackingService.Data.DTOs
{
    public class RouteStopDTO
    {
        public string StationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
    }

    public class RouteDTO
    {
        public string TrainId { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public List<RouteStopDTO> Stops { get; set; } = new List<RouteStopDTO>();
        public double TotalDistance { get; set; }
    }
}
