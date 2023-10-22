using DSaladin.SpeedTime.Integrations;
using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    [DebuggerDisplay("{TrackingStarted.ToString(\"dd.MM.yyyy\")} | {TrackingTime} | IsBreak: {IsBreak}")]
    public class TrackTime : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime TrackingStarted { get; set; }

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

        [NotMapped]
        public bool IsTimeStopped { get => TrackingStopped != default; }

        [NotMapped]
        public double Hours
        {
            get
            {
                double hours;
                if (!IsTimeStopped)
                    hours = DateTime.Now.TimeOfDay.TotalHours - TrackingStarted.TimeOfDay.TotalHours;
                else
                    hours = TrackingStopped.TimeOfDay.TotalHours - TrackingStarted.TimeOfDay.TotalHours;

                if (IsBreak)
                    return hours * -1;

                return hours;
            }
        }

        [NotMapped]
        public string TrackingTime
        {
            get
            {
                string time = TrackingStarted.ToString("HH:mm");

                if (IsTimeStopped)
                    time += " - " + TrackingStopped.ToString("HH:mm");

                return time;
            }
        }

        public string Title { get; set; } = "";
        public bool IsBreak { get; set; } = false;

        [NotMapped]
        public bool IsAFK { get; set; } = false;

        [NotMapped]
        public bool IsJiraTicket { get => JiraService.GetIssueKey(Title) is not null; }

        public virtual List<TrackAttribute> Attributes { get; set; } = new();

        public TrackTime(DateTime trackingStarted, string title, bool isBreak)
        {
            TrackingStarted = trackingStarted;
            Title = title;
            IsBreak = isBreak;
        }

        public void StopTime(DateTime stopTime)
        {
            if (TrackingStarted.Date != stopTime.Date)
                stopTime = TrackingStarted.Date.Add(new(23, 59, 59));

            TrackingStopped = stopTime;
        }

        public void StopTime(TimeSpan? stopTime = null)
        {
            if (stopTime is null && TrackingStarted.Date < DateTime.Today)
                stopTime = new(23, 59, 59);

            stopTime ??= DateTime.Now.TimeOfDay;

            TrackingStopped = TrackingStarted.Date.Add(new TimeSpan(((TimeSpan)stopTime).Hours, ((TimeSpan)stopTime).Minutes, 0));
        }

        public void UpdateTrackingToNow()
        {
            NotifyPropertyChanged(nameof(Hours));
        }

        public static TrackTime Empty()
        {
            return new(DateTime.Today, "", false);
        }

        public void SetAttribute(string key, string value)
        {
            TrackAttribute? trackAttribute = Attributes.FirstOrDefault(ta => ta.Name == key);
            if (trackAttribute is null)
                Attributes.Add(new TrackAttribute()
                {
                    Name = key,
                    Value = value
                });
            else
                trackAttribute.Value = value;

            App.dbContext.Update(this);
            App.dbContext.SaveChanges();
        }

        public string? GetAttribute(string key)
        {
            TrackAttribute? trackAttribute = Attributes.FirstOrDefault(ta => ta.Name == key);
            if (trackAttribute is null)
                return null;

            return trackAttribute.Value;
        }

        public bool ContainsAttribute(string key)
        {
            return GetAttribute(key) is not null;
        }

        public void RemoveAttributes(params string[] keys)
        {
            Attributes.RemoveAll(ta => keys.Contains(ta.Name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
