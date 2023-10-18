using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ISeries[] Series { get; set; } =
        {
            new LineSeries<DateTimePoint>()
        };

        public Axis[] XAxes { get; set; } =
        {
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("M", SpeedTime.Language.SpeedTime.Culture.DateTimeFormat))
        };

        public Axis[] YAxes { get; set; } =
        {
            new Axis()
            {
                Labeler = number => number.ToString("N2"),
                MinLimit = 0
            }
        };

        public TimeStatistics(DateTime start, DateTime end)
        {
            InitializeComponent();
            DataContext = this;

            StartDate = start;
            EndDate = end;

            Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            List<DateTimePoint> points = new();
            List<TrackTime> times = await App.dbContext.TrackedTimes.Where(tt => tt.TrackingStarted >= StartDate && tt.TrackingStopped <= EndDate && !tt.IsBreak).ToListAsync();
            
            for(DateTime current = StartDate; current <= EndDate; current = current.AddDays(1))
            {
                points.Add(new(current, times.Where(tt => tt.TrackingStarted.Date == current).Sum(tt => tt.Hours)));
            }

            Series[0].Values = points;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
