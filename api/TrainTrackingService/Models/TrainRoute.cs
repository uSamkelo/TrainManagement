namespace TrainTrackingService.Models
{
    public class TrainRoute
    {
        public string Id { get; set; } = null!;
        public string RouteName { get; set; } = null!;
        public string LineColor { get; set; } = "#3498db";
        public List<TrainStop> Stops { get; set; } = new();
        public List<TrainSchedule> Schedules { get; set; } = new();
    }
}
