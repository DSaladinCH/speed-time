﻿using DSaladin.FancyPotato;
using DSaladin.FancyPotato.DSWindows;
using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Model;
using GlobalHotKey;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace DSaladin.SpeedTime.ViewModel
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

        private DateRange weekRange = new();
        public string TotalWeekHoursDisplay
        {
            get
            {
                return string.Format("{0:00.00}h / {1:00.00}h", TrackedTimes.Where(tt => tt.TrackingStarted.Date >= weekRange.Start && tt.TrackingStarted <= weekRange.End && !tt.IsBreak).Sum(tt => tt.Hours),
                    SettingsModel.Instance.WeeklyWorkHours);
            }
        }

        private DateTime currentDateTime = DateTime.Today;
        public DateTime CurrentDateTime
        {
            get => currentDateTime;
            set
            {
                currentDateTime = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsCurrentlyTracking { get => CurrentTime != null; }

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
        public RelayCommand AddTrackingCommand { get; set; }
        public RelayCommand StopCurrentTrackingCommand { get; set; }
        public RelayCommand TrackTimeDoubleClickCommand { get; set; }
        public RelayCommand TrackTimeDeleteCommand { get; set; }
        public RelayCommand OpenUserSettingsCommand { get; set; }

        public MainWindowViewModel()
        {
            #region Commands
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

            AddTrackingCommand = new(async (_) =>
            {
                TrackTime? newTime = await ShowDialog<TrackTime>(new TrackTimeEditor());
                if (newTime is not null)
                {
                    await App.dbContext.TrackedTimes.AddAsync(newTime);
                    await App.dbContext.SaveChangesAsync();
                    UpdateView();
                }
            });

            StopCurrentTrackingCommand = new(async (_) =>
            {
                if (CurrentTime is null || CurrentTime.IsTimeStopped)
                    return;

                int id = CurrentTime.Id;
                CurrentTime.StopTime();

                App.dbContext.TrackedTimes.Update(App.dbContext.TrackedTimes.First(t => t.Id == id));
                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            TrackTimeDoubleClickCommand = new(async (sender) =>
            {
                await ShowDialog(new TrackTimeEditor((TrackTime)sender));
                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            TrackTimeDeleteCommand = new(async (sender) =>
            {
                App.dbContext.TrackedTimes.Remove((TrackTime)sender);
                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            OpenUserSettingsCommand = new(async (_) =>
            {
                bool shouldRestart = await ShowDialog<bool>(new UserSettings());
                await App.DataService.SaveSettings();

                if (shouldRestart)
                {
                    Process.Start(Environment.ProcessPath!);
                    Application.Current.Shutdown();
                }
            });
            #endregion

            new Task(async () => await UpdateCurrentTime()).Start();
            new Task(async () => await App.CheckForUpdate()).Start();
            UpdateView();
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
            weekRange = GetWeekRange(CurrentDateTime);
            NotifyPropertyChanged("");
            TrackedTimesViewSource.View?.Refresh();
        }

        async Task UpdateCurrentTime()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentTime?.UpdateTrackingToNow();
                        NotifyPropertyChanged(nameof(TotalHours));
                        NotifyPropertyChanged(nameof(TotalWeekHoursDisplay));
                    });
                }
                catch { }
            }
        }

        private TrackTime? GetCurrentTrackTime()
        {
            TrackTime? lastTime = TrackedTimes.LastOrDefault(t => !t.IsTimeStopped);
            if (lastTime is null || lastTime.IsTimeStopped)
                return null;

            return lastTime;
        }

        public struct DateRange
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public static DateRange GetWeekRange(DateTime date)
        {            
            DayOfWeek firstDayOfWeek = DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;

            int diff = (date.DayOfWeek - firstDayOfWeek + 7) % 7;
            DateTime startOfWeek = date.AddDays(-1 * diff).Date;
            DateTime endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

            return new()
            {
                Start = startOfWeek,
                End = endOfWeek
            };
        }
    }
}