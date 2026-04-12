namespace TrainTrackingService.Models
{
    public class TrainPosition
    {
        public string TrainId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[]? RowVersion { get; set; }
        public string Status { get; set; }
        public TimeSpan ETA { get; set; }
    }
}
