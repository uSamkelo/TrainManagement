using Microsoft.EntityFrameworkCore;
using TrainTrackingService.Models;

namespace TrainTrackingService.Data
{
    public class TrainTrackingContext : DbContext
    {
        public TrainTrackingContext(DbContextOptions<TrainTrackingContext> options) : base(options)
        {
        }

        public DbSet<TrainPosition> TrainPositions { get; set; }
        public DbSet<TrainRoute> TrainRoutes { get; set; }
        public DbSet<TrainSchedule> TrainSchedules { get; set; }
        public DbSet<TrainStop> TrainStops { get; set; }
        public DbSet<ScheduleStop> ScheduleStops { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainPosition>().HasKey(t => t.TrainId);

            // Pin table names to match existing migration
            modelBuilder.Entity<TrainRoute>().ToTable("TrainRoute");
            modelBuilder.Entity<ScheduleStop>().ToTable("ScheduleStop");

            // Configure ScheduleStop relationships - restrict delete to avoid cascade cycles
            modelBuilder.Entity<ScheduleStop>()
                .HasOne<TrainSchedule>()
                .WithMany(s => s.Stops)
                .HasForeignKey(ss => ss.TrainScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScheduleStop>()
                .HasOne(ss => ss.Stop)
                .WithMany()
                .HasForeignKey(ss => ss.StopId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure TrainStop -> TrainRoute to avoid another cascade cycle
            modelBuilder.Entity<TrainStop>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Stops)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
