using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using DSaladin.FancyPotato.DSWindows;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Diagnostics;
using DSaladin.FancyPotato;
using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace DSaladin.SpeedTime.ViewModel
{
    public partial class QuickTimeTrackerViewModel : DSViewModel
    {
        private string workTitle = "";
        public string WorkTitle
        {
            get => workTitle;
            set
            {
                workTitle = value;
                NotifyPropertyChanged();
                DebounceRefreshSuggestions();
            }
        }

        private bool? isBreak = false;
        public bool? IsBreak
        {
            get => isBreak;
            set
            {
                isBreak = value;
                if (IsBreak == true)
                    IsAFK = false;
                NotifyPropertyChanged();
            }
        }

        private bool? isAFK = false;
        public bool? IsAFK
        {
            get => isAFK;
            set
            {
                isAFK = value;
                if (IsAFK == true)
                    IsBreak = false;
                NotifyPropertyChanged();
            }
        }

        private double windowHeight = 110;
        public double WindowHeight
        {
            get => windowHeight + SuggestionsHeight;
        }

        public double SuggestionsHeight
        {
            get
            {
                if (TrackedTimesViewSource.View is null || string.IsNullOrEmpty(WorkTitle))
                    return 0;

                int count = TrackedTimesViewSource.View.Cast<object>().Count();
                return count switch
                {
                    0 => 0,
                    1 => 40,
                    2 => 70,
                    _ => (double)110,
                };
            }
        }

        private TrackTime? lastTrackTime = default;
        public TrackTime? LastTrackTime
        {
            get => lastTrackTime;
            set
            {
                lastTrackTime = value;
                NotifyPropertyChanged();
            }
        }

        public CollectionViewSource TrackedTimesViewSource { get; private set; } = new();

        private int suggestionSelectedIndex = -1;
        public int SuggestionSelectedIndex
        {
            get => suggestionSelectedIndex;
            set
            {
                suggestionSelectedIndex = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand TabButtonCommand { get; set; }
        public RelayCommand UpButtonCommand { get; set; }
        public RelayCommand DownButtonCommand { get; set; }

        private CancellationTokenSource refreshCancellationToken = new();

        public QuickTimeTrackerViewModel(TrackTime? lastTrackTime)
        {
            LastTrackTime = lastTrackTime;

            TabButtonCommand = new((_) =>
            {
                if (TrackedTimesViewSource.View.Cast<object>().Count() == 0)
                    return;

                WorkTitle = TrackedTimesViewSource.View.Cast<TitleMatch>().ElementAt(SuggestionSelectedIndex).Title;
            });

            UpButtonCommand = new((_) =>
            {
                if (SuggestionSelectedIndex <= 0)
                    return;

                SuggestionSelectedIndex--;
            });

            DownButtonCommand = new((_) =>
            {
                if (SuggestionSelectedIndex >= TrackedTimesViewSource.View.Cast<object>().Count() - 1)
                    return;

                SuggestionSelectedIndex++;
            });
        }

        public override void WindowContentRendered(object? sender, EventArgs eventArgs)
        {
            DSWindow window = (DSWindow)sender!;
            SetForegroundWindow(new WindowInteropHelper(window).Handle);
            window.KeyUp += QuickTimeTracker_KeyUp;
        }

        public override async void WindowLoaded(object sender, RoutedEventArgs eventArgs)
        {
            TrackedTimesViewSource = new();
            TrackedTimesViewSource.SetCurrentValue(CollectionViewSource.SourceProperty,
                    (await App.dbContext.TrackedTimes.OrderByDescending(t => t.Id).AsNoTracking()
                        .Take(SettingsModel.Instance.SearchNumberOfItems).ToListAsync())
                            .Select(t => new TitleMatch() { Title = t.Title }));

            TrackedTimesViewSource.Filter += TrackedTimesViewSource_Filter;
            TrackedTimesViewSource.SortDescriptions.Add(new("MatchPercentage", ListSortDirection.Descending));
        }

        private void TrackedTimesViewSource_Filter(object sender, FilterEventArgs e)
        {
            TitleMatch titleMatch = (e.Item as TitleMatch)!;
            e.Accepted = titleMatch.MatchPercentage > 0;
        }

        private async void DebounceRefreshSuggestions(int debounceTime = 200)
        {
            refreshCancellationToken.Cancel();
            refreshCancellationToken = new();

            try
            {
                await Task.Delay(debounceTime, refreshCancellationToken.Token);
                RefreshSuggestions();
            }
            catch (TaskCanceledException) { }
        }

        private void RefreshSuggestions()
        {
            Debug.WriteLine("Refreshing");

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (TrackedTimesViewSource.View is null)
                    return;

                List<TitleMatch> matches = TrackedTimesViewSource.View.SourceCollection.Cast<TitleMatch>().ToList();
                foreach (TitleMatch titleMatch in matches)
                    titleMatch.CalculateMatchPercentage(WorkTitle);

                TrackedTimesViewSource.SetCurrentValue(CollectionViewSource.SourceProperty, matches);
                TrackedTimesViewSource.View.Refresh();

                NotifyPropertyChanged(nameof(TrackedTimesViewSource));
                NotifyPropertyChanged(nameof(SuggestionsHeight));
                NotifyPropertyChanged(nameof(WindowHeight));
            });
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        private void QuickTimeTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WorkTitle = "";
                (sender as Window)!.Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                (sender as Window)!.Close();
                return;
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.B)
                {
                    IsBreak = !IsBreak;
                    return;
                }

                if (e.Key == Key.F)
                {
                    IsAFK = !isAFK;
                    return;
                }
            }
        }
    }
}
