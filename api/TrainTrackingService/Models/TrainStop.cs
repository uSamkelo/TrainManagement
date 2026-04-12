namespace TrainTrackingService.Models
{
    public class TrainStop
    {
        public int Id { get; set; }
        public string StationName { get; set; } = null!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int StopOrder { get; set; }
        public double DistanceFromStart { get; set; } // km
        public string RouteId { get; set; } = null!;
        public TrainRoute? Route { get; set; }
    }
}
