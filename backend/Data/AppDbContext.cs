using Microsoft.EntityFrameworkCore;
using Tour_Project.Models;

namespace Tour_Project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LocationManagerAssignment> LocationManagerAssignments { get; set; }
        public DbSet<LocationStat> LocationStats { get; set; }

        // Tour Management
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLocation> TourLocations { get; set; }
        public DbSet<TourSession> TourSessions { get; set; }
        public DbSet<SessionVisit> SessionVisits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TourLocation>()
                .HasIndex(tl => new { tl.TourId, tl.LocationId })
                .IsUnique();

            modelBuilder.Entity<TourLocation>()
                .HasOne(tl => tl.Tour)
                .WithMany(t => t.TourLocations)
                .HasForeignKey(tl => tl.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TourLocation>()
                .HasOne(tl => tl.Location)
                .WithMany()
                .HasForeignKey(tl => tl.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionVisit>()
                .HasOne(sv => sv.Session)
                .WithMany(s => s.Visits)
                .HasForeignKey(sv => sv.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
