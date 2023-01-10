using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker
{
    internal class TimeTrackerContext : DbContext
    {
        public DbSet<TrackTime> TrackedTimes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=timetracker.db");
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
