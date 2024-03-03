using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSaladin.SpeedTime.Dialogs
{
    public partial class TimeStatistics : DSDialogControl
    {
        private DateTime initialStartDate;
        private DateTime initialEndDate;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private StatisticsGrouping rangeGrouping;
        public StatisticsGrouping RangeGrouping
        {
            get => rangeGrouping;
            set
            {
                rangeGrouping = value;
                NotifyPropertyChanged();
            }
        }

        private string rangeDisplay = "";
        public string RangeDisplay
        {
            get => rangeDisplay; set
            {
                rangeDisplay = value;
                NotifyPropertyChanged();
            }
        }

        public List<ISeries> Series { get; set; }

        private Axis[] xAxes =
        {
            new DateTimeAxis(TimeSpan.FromDays(2), date => ((App)Application.Current).FormatDate(date, "M"))
        };
        public Axis[] XAxes
        {
            get => xAxes;
            set
            {
                xAxes = value;
                NotifyPropertyChanged();
            }
        }

        public Axis[] YAxes { get; set; } =
        {
            new Axis()
            {
                Labeler = number => number.ToString("N2"),
                MinLimit = 0
            }
        };

        public RelayCommand RangeBackCommand { get; set; }
        public RelayCommand RangeNextCommand { get; set; }

        public RelayCommand GroupingBackCommand { get; set; }
        public RelayCommand GroupingNextCommand { get; set; }

        public RelayCommand RangeDoubleClickCommand { get; set; }

        public TimeStatistics(DateTime start, DateTime end)
        {
            InitializeComponent();

            Series = new() {
                new LineSeries<DateTimePoint>() { ScalesYAt = 0, Name = SpeedTime.Language.SpeedTime.statistics_legends_actual },
                new LineSeries<DateTimePoint>() { ScalesYAt = 0, Name = SpeedTime.Language.SpeedTime.statistics_legends_target }
            };

            DataContext = this;

            initialStartDate = start;
            initialEndDate = end;

            StartDate = start;
            EndDate = end;

            RangeBackCommand = new(async (_) =>
            {
                switch (RangeGrouping)
                {
                    case StatisticsGrouping.WeekDaily:
                        StartDate = StartDate.AddDays(-7);
                        EndDate = EndDate.AddDays(-7);
                        break;
                    case StatisticsGrouping.MonthDaily:
                    case StatisticsGrouping.MonthWeekly:
                        StartDate = StartDate.AddMonths(-1);
                        EndDate = StartDate.AddMonths(1).AddDays(-1);
                        break;
                }
                await LoadData();
            });

            RangeNextCommand = new(async (_) =>
            {
                switch (RangeGrouping)
                {
                    case StatisticsGrouping.WeekDaily:
                        StartDate = StartDate.AddDays(7);
                        EndDate = EndDate.AddDays(7);
                        break;
                    case StatisticsGrouping.MonthDaily:
                    case StatisticsGrouping.MonthWeekly:
                        StartDate = StartDate.AddMonths(1);
                        EndDate = EndDate.AddMonths(1);
                        break;
                }
                await LoadData();
            });

            GroupingBackCommand = new(async (_) =>
            {
                RangeGrouping = (StatisticsGrouping)(((int)RangeGrouping - 1 + Enum.GetValues(typeof(StatisticsGrouping)).Length) % Enum.GetValues(typeof(StatisticsGrouping)).Length);
                UpdateDatesAfterGrouping();
                await LoadData();
            });

            GroupingNextCommand = new(async (_) =>
            {
                RangeGrouping = (StatisticsGrouping)(((int)RangeGrouping + 1) % Enum.GetValues(typeof(StatisticsGrouping)).Length);
                UpdateDatesAfterGrouping();
                await LoadData();
            });

            RangeDoubleClickCommand = new(async (_) =>
            {
                switch (RangeGrouping)
                {
                    case StatisticsGrouping.WeekDaily:
                        StartDate = initialStartDate;
                        EndDate = initialEndDate;
                        break;
                    case StatisticsGrouping.MonthDaily:
                    case StatisticsGrouping.MonthWeekly:
                        StartDate = new(initialStartDate.Year, initialStartDate.Month, 1);
                        EndDate = StartDate.AddMonths(1).AddDays(-1);
                        break;
                }

                await LoadData();
            });

            Loaded += async (s, e) =>
            {
                UpdateDatesAfterGrouping();
                await LoadData();
            };
        }

        private void UpdateDatesAfterGrouping()
        {
            switch (RangeGrouping)
            {
                case StatisticsGrouping.WeekDaily:
                    StartDate = initialStartDate;
                    EndDate = initialEndDate;
                    break;
                case StatisticsGrouping.MonthDaily:
                case StatisticsGrouping.MonthWeekly:
                    StartDate = new(StartDate.Year, StartDate.Month, 1);
                    EndDate = StartDate.AddMonths(1).AddDays(-1);
                    break;
            }

            string dailyFormat = SpeedTime.Language.SpeedTime.ResourceManager.GetString("statistics.daily-date-format", ((App)Application.Current).CurrentDateLanguage)!;
            switch (RangeGrouping)
            {
                case StatisticsGrouping.WeekDaily:
                    XAxes = [new DateTimeAxis(TimeSpan.FromDays(2), date => ((App)Application.Current).FormatDate(date, dailyFormat))];
                    break;
                case StatisticsGrouping.MonthDaily:
                    XAxes = [new DateTimeAxis(TimeSpan.FromDays(7), date => ((App)Application.Current).FormatDate(date, dailyFormat))];
                    break;
                case StatisticsGrouping.MonthWeekly:
                    XAxes = [ new DateTimeAxis(TimeSpan.FromDays(7),
                        date => string.Format(SpeedTime.Language.SpeedTime.statistics_week_placeholder, GetIso8601WeekOfYear(date))) {
                            LabelsAlignment = LiveChartsCore.Drawing.Align.End
                        }
                    ];
                    break;
            }
        }

        private async Task LoadData()
        {
            switch (RangeGrouping)
            {
                case StatisticsGrouping.WeekDaily:
                    RangeDisplay = string.Format(SpeedTime.Language.SpeedTime.statistics_week_placeholder, GetIso8601WeekOfYear(StartDate));
                    break;
                case StatisticsGrouping.MonthDaily:
                case StatisticsGrouping.MonthWeekly:
                    RangeDisplay = ((App)Application.Current).FormatDate(StartDate, "MMMM");
                    break;
            }

            List<DateTimePoint> targetWork = new();
            List<DateTimePoint> actualWork = new();
            List<TrackTime> times = await App.dbContext.TrackedTimes.Where(tt => tt.TrackingStarted.Date >= StartDate && tt.TrackingStopped.Date <= EndDate && !tt.IsBreak).ToListAsync();

            switch (RangeGrouping)
            {
                case StatisticsGrouping.WeekDaily:
                case StatisticsGrouping.MonthDaily:
                    for (DateTime current = StartDate; current <= EndDate; current = current.AddDays(1))
                    {
                        double targetWorkhours = SettingsModel.Instance.Workdays.GetWorkHours(current);
                        double actualWorkhours = times.Where(tt => tt.TrackingStarted.Date == current).Sum(tt => tt.Hours);

                        // if the current date is not a work day and no hours were worked, don't display it
                        if (targetWorkhours == 0 && actualWorkhours == 0)
                            continue;

                        actualWork.Add(new(current, actualWorkhours));
                        targetWork.Add(new(current, targetWorkhours));
                    }
                    break;
                case StatisticsGrouping.MonthWeekly:
                    // Find the closest preceding Monday for the start date
                    DateTime currentStartOfWeek = StartDate;
                    while (currentStartOfWeek.DayOfWeek != DayOfWeek.Monday)
                    {
                        currentStartOfWeek = currentStartOfWeek.AddDays(-1);
                    }

                    while (currentStartOfWeek <= EndDate)
                    {
                        DateTime currentEndOfWeek = currentStartOfWeek.AddDays(6);  // Move to Sunday

                        // Adjust dates to fit within the range
                        DateTime effectiveStart = (currentStartOfWeek < StartDate) ? StartDate : currentStartOfWeek;
                        DateTime effectiveEnd = (currentEndOfWeek > EndDate) ? EndDate : currentEndOfWeek;

                        double targetWorkhours = SettingsModel.Instance.Workdays.GetWorkHoursForRange(effectiveStart, effectiveEnd);
                        double actualWorkhours = times.Where(tt => tt.TrackingStarted.Date >= effectiveStart && tt.TrackingStarted.Date <= effectiveEnd).Sum(tt => tt.Hours);

                        // if the current date is not a work day and no hours were worked, don't display it
                        if (targetWorkhours == 0 && actualWorkhours == 0)
                            continue;

                        actualWork.Add(new(currentStartOfWeek, actualWorkhours));
                        targetWork.Add(new(currentStartOfWeek, targetWorkhours));

                        // Move to the start of the next week
                        currentStartOfWeek = currentStartOfWeek.AddDays(7);
                    }
                    break;
            }

            Series[0].Values = actualWork;
            Series[1].Values = targetWork;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private int GetIso8601WeekOfYear(DateTime date)
        {
            System.Globalization.Calendar calendar = ((App)Application.Current).CurrentDateLanguage.Calendar;

            // ISO 8601 specifies that the first week of the year is the one with January 4th
            // or the week that includes the first Thursday of the year
            DayOfWeek day = calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                date = date.AddDays(3);

            // Return the week of our adjusted day
            return calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, ((App)Application.Current).CurrentDateLanguage.DateTimeFormat.FirstDayOfWeek);
        }

        public enum StatisticsGrouping
        {
            [Description("statistics.grouping.week-daily")]
            WeekDaily = 0,
            [Description("statistics.grouping.month-daily")]
            MonthDaily = 1,
            [Description("statistics.grouping.month-weekly")]
            MonthWeekly = 2
        }
    }
}
