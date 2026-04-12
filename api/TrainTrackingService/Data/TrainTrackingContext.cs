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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainPosition>().HasKey(t => t.TrainId);
        }
    }
}
