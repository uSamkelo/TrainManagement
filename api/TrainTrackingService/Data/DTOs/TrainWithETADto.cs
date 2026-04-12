namespace TrainTrackingService.Data.DTOs
{
    public class TrainWithETADto
    {
        public string TrainId { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = null!;
        public string? CurrentStop { get; set; }
        public string? NextStop { get; set; }
        public DateTime? NextStopArrivalTime { get; set; }
        public TimeSpan? ETAToNextStop { get; set; }
        public DateTime? SelectedStationArrivalTime { get; set; }
        public TimeSpan? ETAToSelectedStation { get; set; }
        public List<UpcomingStopDto> UpcomingStops { get; set; } = new();
    }

    public class UpcomingStopDto
    {
        public string StationName { get; set; } = null!;
        public DateTime ArrivalTime { get; set; }
        public TimeSpan ETA { get; set; }
    }
}
