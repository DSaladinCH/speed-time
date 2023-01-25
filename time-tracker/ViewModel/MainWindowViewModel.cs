using DSaladin.FancyPotato;
using DSaladin.FancyPotato.DSWindows;
using DSaladin.TimeTracker.Dialogs;
using DSaladin.TimeTracker.Model;
using GlobalHotKey;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace DSaladin.TimeTracker.ViewModel
{
    public class MainWindowViewModel : DSViewModel
    {
        private ObservableCollection<TrackTime> trackedTimes = new();
        public ObservableCollection<TrackTime> TrackedTimes
        {
            get { return trackedTimes; }
            set
            {
                trackedTimes = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentTime));
            }
        }

        public CollectionViewSource TrackedTimesViewSource { get; private set; } = new();

        public TrackTime? CurrentTime { get => GetCurrentTrackTime(); }
        public double TotalHours { get => TrackedTimes.Where(tt => tt.TrackingStarted.Date == CurrentDateTime.Date && !tt.IsBreak).Sum(tt => tt.Hours); }

        private DateTime currentTime = DateTime.Today;
        public DateTime CurrentDateTime
        {
            get => currentTime;
            set
            {
                currentTime = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsCurrentDateTimeFreeDayChangeable { get; private set; }
        public bool IsCurrentDateTimeFreeDay
        {
            get
            {
                if (CurrentDateTime.DayOfWeek == DayOfWeek.Saturday || CurrentDateTime.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    IsCurrentDateTimeFreeDayChangeable = false;
                    return true;
                }

                IsCurrentDateTimeFreeDayChangeable = true;
                return SettingsModel.Instance.SpecialWorkHours.Any(sph => sph.Item1 == CurrentDateTime && sph.Item2 == 0);
            }
            set
            {
                if (value)
                    SettingsModel.Instance.SpecialWorkHours.Add(new(CurrentDateTime, 0));
                else
                    SettingsModel.Instance.SpecialWorkHours.Remove(new(CurrentDateTime, 0));
            }
        }

        public RelayCommand ChangeCurrentDateTimeCommand { get; set; }
        public RelayCommand CurrentDateTimeDoubleClickCommand { get; set; }
        public RelayCommand StopCurrentTrackingCommand { get; set; }
        public RelayCommand OpenUserSettingsCommand { get; set; }

        public MainWindowViewModel()
        {
            ChangeCurrentDateTimeCommand = new((parameter) =>
            {
                if (parameter is null)
                    return;

                CurrentDateTime = CurrentDateTime.AddDays(int.Parse((string)parameter));
                UpdateView();
            });

            CurrentDateTimeDoubleClickCommand = new((_) =>
            {
                CurrentDateTime = DateTime.Today;
                UpdateView();
            });

            StopCurrentTrackingCommand = new(async (_) =>
            {
                TrackTime? lastTrackedTime = await App.dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();
                if (lastTrackedTime is null)
                    return;

                lastTrackedTime.StopTime();
                App.dbContext.TrackedTimes.Update(lastTrackedTime);
                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            OpenUserSettingsCommand = new(async (window) =>
            {
                // TODO: Call ShowDialog from ViewModel
                await ShowDialog(new UserSettings());
                await App.DataService.SaveSettings();
            });

            UpdateCurrentTime();
        }

        public override void WindowLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // load the entities into EF Core
            App.dbContext.TrackedTimes.Load();

            // bind to the source
            TrackedTimesViewSource = new();
            TrackedTimesViewSource.Source = App.dbContext.TrackedTimes.Local.ToObservableCollection();
            TrackedTimesViewSource.Filter += (s, e) => e.Accepted = (e.Item as TrackTime)!.TrackingStarted.Date == CurrentDateTime.Date;
            TrackedTimesViewSource.SortDescriptions.Add(new("TrackingStarted", ListSortDirection.Descending));
            TrackedTimesViewSource.SortDescriptions.Add(new("TrackingStopped", ListSortDirection.Descending));
            TrackedTimes = App.dbContext.TrackedTimes.Local.ToObservableCollection();
            App.dbContext.SavedChanges += (s, e) => UpdateView();

            UpdateView();
        }

        private void UpdateView()
        {
            NotifyPropertyChanged("");
            TrackedTimesViewSource.View.Refresh();
        }

        async Task UpdateCurrentTime()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTime?.UpdateTrackingToNow();
                    NotifyPropertyChanged(nameof(TotalHours));
                });
            }
        }

        private TrackTime? GetCurrentTrackTime()
        {
            TrackTime? lastTime = TrackedTimes.LastOrDefault();
            if (lastTime is null || lastTime.IsTimeStopped)
                return null;

            return lastTime;
        }
    }
}
