using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker
{
    public class TrackTime
    {
        public DateTime TrackingStarted { get; set; }
        public DateTime TrackingStopped { get; set; }
        public double Hours
        {
            get
            {
                return TrackingStopped.TimeOfDay.TotalHours - TrackingStarted.TimeOfDay.TotalHours;
            }
        }
        public string Title { get; set; }
        public bool IsBreak { get; set; }

        public TrackTime(DateTime trackingStarted, string title, bool isBreak)
        {
            TrackingStarted = trackingStarted;
            Title = title;
            IsBreak = isBreak;
        }
    }
}
