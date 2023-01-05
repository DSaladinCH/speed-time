using GlobalHotKey;
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
using System.Windows.Input;

namespace DSaladin.TimeTracker
{
    public class MainWindowViewModel : INotifyPropertyChanged
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

        public TrackTime? CurrentTime { get => TrackedTimes.LastOrDefault(); }
        public double TotalHours { get => TrackedTimes.Where(tt => !tt.IsBreak).Sum(tt => tt.Hours); }

        public MainWindowViewModel()
        {
            openQuickTimeTracker = hotKeyManager.Register(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;

            //TrackedTimes.Add(new(DateTime.Now.AddHours(-8).AddMinutes(-27), "Something, don't know...", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-5).AddMinutes(-14).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-5).AddMinutes(-14), "Programming", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-2).AddMinutes(-32).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-2).AddMinutes(-32), "Fixing bugs", false));
            //TrackedTimes.Last().StopTime(DateTime.Now.AddHours(-0).AddMinutes(-58).TimeOfDay);
            //TrackedTimes.Add(new(DateTime.Now.AddHours(-0).AddMinutes(-58), "Helping Customer on phone", false));

            UpdateCurrentTime();
        }

        private void HotKeyManagerPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Equals(openQuickTimeTracker))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TrackTime? trackTime = QuickTimeTracker.Open(Application.Current.MainWindow);
                    if (trackTime is null)
                        return;

                    if (TrackedTimes.Count > 0)
                        TrackedTimes.Last().StopTime();
                    TrackedTimes.Add(trackTime);
                    NotifyPropertyChanged(nameof(CurrentTime));
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
