using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker
{
    public class TrackTime : INotifyPropertyChanged
    {
        public DateTime TrackingStarted { get; set; }
        public double TrackingToNow { get => DateTime.Now.TimeOfDay.TotalHours - TrackingStarted.TimeOfDay.TotalHours; }

        private DateTime trackingStopped = new();
        public DateTime TrackingStopped
        {
            get => trackingStopped;
            private set
            {
                trackingStopped = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Hours));
                NotifyPropertyChanged(nameof(TrackingTime));
            }
        }

        public double Hours
        {
            get
            {
                return TrackingStopped.TimeOfDay.TotalHours - TrackingStarted.TimeOfDay.TotalHours;
            }
        }

        public string TrackingTime
        {
            get
            {
                string time = TrackingStarted.ToString("HH:mm");

                if (TrackingStopped != default)
                    time += " - " + TrackingStopped.ToString("HH:mm");

                return time;
            }
        }
        public string Title { get; set; } = "";
        public bool IsBreak { get; set; } = false;

        public TrackTime(DateTime trackingStarted, string title, bool isBreak)
        {
            TrackingStarted = trackingStarted;
            Title = title;
            IsBreak = isBreak;
        }

        public void StopTime(TimeSpan? stopTime)
        {
            if (stopTime is null)
                stopTime = DateTime.Now.TimeOfDay;

            TrackingStopped = TrackingStarted.Date.Add((TimeSpan)stopTime);
        }

        public void UpdateTrackingToNow()
        {
            NotifyPropertyChanged(nameof(TrackingToNow));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
