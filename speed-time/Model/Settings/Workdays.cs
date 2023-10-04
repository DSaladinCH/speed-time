using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal class Workdays : INotifyPropertyChanged
    {
        public List<Workday> Config { get; set; } = new();

        internal void SetWorkHours(DateTime startDate, DateTime endDate, DayOfWeek dayOfWeek, double workHours)
        {
            Workday? workday = Config.FirstOrDefault(c => c.DayOfWeek == dayOfWeek);
            if (workday is null)
            {
                Config.Add(new Workday() { StartDate = startDate, EndDate = endDate, DayOfWeek = dayOfWeek, WorkHours = workHours });
                return;
            }

            int index = Config.IndexOf(workday);
            workday.WorkHours = workHours;
            Config[index] = workday;
        }

        /// <summary>
        /// Gets the work hours of a specific date
        /// </summary>
        /// <param name="dateTime">The date that should be checked</param>
        /// <returns>Returns the number of work hours for the specific date</returns>
        internal double GetWorkHours(DateTime dateTime)
        {
            if (Config.Exists(x => x.DayOfWeek == dateTime.DayOfWeek))
                return Config.First(x => x.DayOfWeek == dateTime.DayOfWeek).WorkHours;

            return 0;
        }

        /// <summary>
        /// Gets the work hours of a specific day of the week, that is active for the current date
        /// </summary>
        /// <param name="dayOfWeek">The day of the week that should be checked</param>
        /// <returns>Returns the number of work hours for the specific day of the week</returns>
        internal double GetWorkHours(DayOfWeek dayOfWeek)
        {
            if (Config.Exists(x => x.DayOfWeek == dayOfWeek))
                return Config.First(x => x.DayOfWeek == dayOfWeek).WorkHours;

            return 0;
        }

        internal double GetWeekWorkHours(DateTime startDate, DateTime endDate)
        {
            // Ignore start and end date atm
            return GetWorkHours(DayOfWeek.Monday)
                + GetWorkHours(DayOfWeek.Tuesday)
                + GetWorkHours(DayOfWeek.Wednesday)
                + GetWorkHours(DayOfWeek.Thursday)
                + GetWorkHours(DayOfWeek.Friday)
                + GetWorkHours(DayOfWeek.Saturday)
                + GetWorkHours(DayOfWeek.Sunday);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        internal class Workday
        {
            public DayOfWeek DayOfWeek { get; set; }
            public double WorkHours { get; set; }

            // Not used at the moment, but for future feature -> Workhours could change, ex. workload change
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
