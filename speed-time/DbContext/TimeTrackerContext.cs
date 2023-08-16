using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime
{
    internal class TimeTrackerContext : DbContext
    {
        public DbSet<TrackTime> TrackedTimes { get; set; }
        public DbSet<TrackAttribute> TrackAttributes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=timetracker.db");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define composite primary key for TrackAttribute
            modelBuilder.Entity<TrackAttribute>()
                .HasKey(t => new { t.TrackTimeId, t.Name });

            // Define relationship between TrackTime and TrackAttribute
            modelBuilder.Entity<TrackAttribute>()
                .HasOne(ta => ta.TrackTime)
                .WithMany(tt => tt.Attributes)
                .HasForeignKey(ta => ta.TrackTimeId);
        }
    }
}
