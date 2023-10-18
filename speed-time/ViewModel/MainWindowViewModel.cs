using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.FancyPotato.DSUserControls;
using DSaladin.FancyPotato.DSWindows;
using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Integrations;
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
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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
        public string TotalHours
        {
            get
            {
                double hours = TrackedTimes.Where(tt => tt.TrackingStarted.Date == CurrentDateTime.Date && !tt.IsBreak).Sum(tt => tt.Hours);
                return string.Format("{0:00.00}h / {1:00.00}h", hours, SettingsModel.Instance.Workdays.GetWorkHours(currentDateTime));
            }
        }

        private DateRange weekRange = new();
        public string TotalWeekHoursDisplay
        {
            get
            {
                return string.Format("{0:00.00}h / {1:00.00}h", TrackedTimes.Where(tt => tt.TrackingStarted.Date >= weekRange.Start && tt.TrackingStarted <= weekRange.End && !tt.IsBreak).Sum(tt => tt.Hours),
                    SettingsModel.Instance.Workdays.GetWorkHoursForRange(weekRange.Start, weekRange.End));
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

        public bool IsTrackTimeEditorOpen { get; set; }

        private bool isJiraLoading;
        public bool IsJiraLoading
        {
            get { return isJiraLoading; }
            set
            {
                isJiraLoading = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand ChangeCurrentDateTimeCommand { get; set; }
        public RelayCommand CurrentDateTimeDoubleClickCommand { get; set; }
        public RelayCommand UpdateJiraCommand { get; set; }
        public RelayCommand AddTrackingCommand { get; set; }
        public RelayCommand StopCurrentTrackingCommand { get; set; }
        public RelayCommand TrackTimeDoubleClickCommand { get; set; }
        public RelayCommand TrackTimeDeleteCommand { get; set; }
        public RelayCommand OpenJiraIssueCommand { get; set; }
        public RelayCommand OpenUserSettingsCommand { get; set; }
        public RelayCommand OpenTimeStatisticsCommand { get; set; }

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

            UpdateJiraCommand = new(async (sender) =>
            {
                // TODO: Check if Jira Enabled
                IsJiraLoading = true;
                await JiraService.UploadWorklogsAsync(TrackedTimesViewSource.View.Cast<TrackTime>().ToList());
                IsJiraLoading = false;
            });

            AddTrackingCommand = new(async (_) =>
            {
                if (IsTrackTimeEditorOpen)
                    return;

                IsTrackTimeEditorOpen = true;
                TrackTime? newTime = await ShowDialog<TrackTime>(new TrackTimeEditor());
                IsTrackTimeEditorOpen = false;

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
                if (IsTrackTimeEditorOpen)
                    return;

                IsTrackTimeEditorOpen = true;
                await ShowDialog(new TrackTimeEditor((TrackTime)sender));
                IsTrackTimeEditorOpen = false;

                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            TrackTimeDeleteCommand = new(async (sender) =>
            {
                TrackTime trackTime = (TrackTime)sender;

                if (SettingsModel.Instance.JiraIsEnabled)
                {
                    ApiLogEntry? logEntry = await JiraService.DeleteWorklogAsync(trackTime);

                    if (logEntry is not null && !logEntry.IsSuccess)
                    {
                        ShowSnackbar("Jira deletion failed", "Delete Anyway", async () =>
                        {
                            App.dbContext.TrackedTimes.Remove(trackTime);
                            await App.dbContext.SaveChangesAsync();
                            UpdateView();

                            ShowSnackbar("Delete worklog manually");
                        });

                        return;
                    }

                    ShowSnackbar("Jira worklog deleted");
                }

                App.dbContext.TrackedTimes.Remove(trackTime);
                await App.dbContext.SaveChangesAsync();
                UpdateView();
            });

            OpenJiraIssueCommand = new(async (sender) =>
            {
                string? jiraIssueKey = JiraService.GetIssueKey(((TrackTime)sender).Title);

                if (jiraIssueKey is null)
                    return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = await JiraService.GetIssueUriAsync(jiraIssueKey),
                    UseShellExecute = true
                });
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

            OpenTimeStatisticsCommand = new(async (_) =>
            {
                await ShowDialog<TimeStatistics>(new TimeStatistics(weekRange.Start, weekRange.End));
            });
            #endregion

            new Task(async () => await UpdateCurrentTime()).Start();
            new Task(async () => await App.CheckForUpdate()).Start();
            UpdateView();
        }

        public override void WindowLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // load the entities into EF Core
            App.dbContext.TrackedTimes.Include(t => t.Attributes).Load();

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