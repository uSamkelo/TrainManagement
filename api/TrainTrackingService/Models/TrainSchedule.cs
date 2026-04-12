namespace TrainTrackingService.Models
{
    public class TrainSchedule
    {
        public int Id { get; set; }
        public string TrainId { get; set; } = null!;
        public string RouteId { get; set; } = null!;
        public TrainRoute? Route { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public List<ScheduleStop> Stops { get; set; } = new();
    }

    public class ScheduleStop
    {
        public int Id { get; set; }
        public int StopId { get; set; }
        public int TrainScheduleId { get; set; } // Foreign key
        public TrainStop? Stop { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan DepartureTime { get; set; }
    }
}
