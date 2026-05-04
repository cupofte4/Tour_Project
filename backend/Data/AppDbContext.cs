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
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<LocationManagerAssignment> LocationManagerAssignments { get; set; }
        public DbSet<LocationStat> LocationStats { get; set; }

        // Tour Management
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLocation> TourLocations { get; set; }
        public DbSet<TourSession> TourSessions { get; set; }
        public DbSet<SessionVisit> SessionVisits { get; set; }
        // Analytics
        public DbSet<AppUsageHeartbeat> AppUsageHeartbeats { get; set; }
        public DbSet<AudioPlay> AudioPlays { get; set; }
        public DbSet<FavoriteClick> FavoriteClicks { get; set; }
        public DbSet<VisitorDevice> VisitorDevices { get; set; }
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }
        public DbSet<LocationFavoriteState> LocationFavoriteStates { get; set; }

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

            modelBuilder.Entity<TourSession>()
                .HasIndex(ts => ts.TourId);

            modelBuilder.Entity<Location>()
                .HasIndex(l => l.Name);

            modelBuilder.Entity<Location>()
                .Property(l => l.Prio)
                .HasMaxLength(20)
                .HasDefaultValue(LocationPriority.DefaultPrio);

            modelBuilder.Entity<LocationManagerAssignment>()
                .HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LocationManagerAssignment>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUsageHeartbeat>()
                .HasIndex(x => new { x.DeviceId, x.OccurredAtUtc });

            modelBuilder.Entity<VisitorDevice>()
                .HasIndex(x => x.DeviceId)
                .IsUnique();

            modelBuilder.Entity<VisitorDevice>()
                .Property(x => x.DeviceId)
                .HasMaxLength(128);

            modelBuilder.Entity<VisitorDevice>()
                .Property(x => x.LastPath)
                .HasMaxLength(1024);

            modelBuilder.Entity<VisitorDevice>()
                .Property(x => x.LastUserAgent)
                .HasMaxLength(512);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.DeviceId)
                .HasMaxLength(128);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.EventType)
                .HasMaxLength(64);

            modelBuilder.Entity<AnalyticsEvent>()
                .Property(x => x.Path)
                .HasMaxLength(1024);

            modelBuilder.Entity<LocationFavoriteState>()
                .HasIndex(x => new { x.DeviceId, x.LocationId })
                .IsUnique();

            modelBuilder.Entity<LocationFavoriteState>()
                .Property(x => x.DeviceId)
                .HasMaxLength(128);
        }
    }
}
