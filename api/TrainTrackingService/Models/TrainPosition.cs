namespace TrainTrackingService.Models
{
    public class TrainPosition
    {
        public string? TrainId { get; set; } = null;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[]? RowVersion { get; set; }
        public string? Status { get; set; } = null;
        public TimeSpan ETA { get; set; }
        public TimeSpan? NextStopETA { get; set; } = null;
        public string? CurrentStop { get; set; } = null;
        public string? NextStop { get; set; } = null;
    }
}
