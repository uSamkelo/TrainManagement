namespace TrainTrackingService.Data.DTOs
{
    public class NextArrivalResult
    {
        public string? TrainId { get; set; }
        public string? RouteName { get; set; }
        public string? StationName { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public TimeSpan? ETA { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
