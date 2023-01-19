﻿using DSaladin.FancyPotato.DSWindows;
using GlobalHotKey;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        readonly HotKeyManager hotKeyManager = new();
        readonly HotKey openQuickTimeTracker;

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

        public RelayCommand ChangeCurrentDateTimeCommand { get; set; }
        public RelayCommand CurrentDateTimeDoubleClickCommand { get; set; }
        public RelayCommand StopCurrentTrackingCommand { get; set; }

        public MainWindowViewModel()
        {
            //openQuickTimeTracker = hotKeyManager.Register(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
            //hotKeyManager.KeyPressed += HotKeyManagerPressed;

            //TrackedTimes.Add(new(DateTime.Now.AddHours(-8).AddMinutes(-27), "Something, don't know...", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-5).AddMinutes(-14).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-5).AddMinutes(-14), "Programming", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-2).AddMinutes(-32).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-2).AddMinutes(-32), "Fixing bugs", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-0).AddMinutes(-58).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-0).AddMinutes(-58), "Helping Customer on phone", false));

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
            TrackedTimes = App.dbContext.TrackedTimes.Local.ToObservableCollection();
            App.dbContext.SavedChanges += (s, e) => UpdateView();

            UpdateView();
        }

        private void UpdateView()
        {
            NotifyPropertyChanged("");
            TrackedTimesViewSource.View.Refresh();
        }

        private void HotKeyManagerPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Equals(openQuickTimeTracker))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //TrackTime? trackTime = QuickTimeTracker.Open(Application.Current.MainWindow, CurrentTime);
                    //new QuickTimeTracker(CurrentTime).ShowDialog();
                    //if (trackTime is null)
                    //    return;

                    //if (TrackedTimes.Count > 0)
                    //    TrackedTimes.Last().StopTime();
                    //TrackedTimes.Add(trackTime);
                    //NotifyPropertyChanged(nameof(CurrentTime));
                });
            }
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
            if (lastTime is null || lastTime.TrackingStopped != default)
                return null;

            return lastTime;
        }
    }
}
