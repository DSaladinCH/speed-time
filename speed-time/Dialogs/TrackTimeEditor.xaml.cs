using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// <summary>
    /// Interaction logic for TrackTimeEditor.xaml
    /// </summary>
    public partial class TrackTimeEditor : DSDialogControl
    {
        private TrackTime trackTime = TrackTime.Empty();

        public TrackTime TrackTime
        {
            get { return trackTime; }
            set
            {
                trackTime = value;
                trackTimeTitle = value.Title;
                SelectedDate = value.TrackingStarted;
                TrackingStarted = value.TrackingStarted;
                TrackingStopped = value.TrackingStopped;
                IsBreak = value.IsBreak;
                NotifyPropertyChanged("");
            }
        }

        private string trackTimeTitle = "";
        public string TrackTimeTitle
        {
            get { return trackTimeTitle; }
            set
            {
                trackTimeTitle = value;
                NotifyPropertyChanged();
                DebounceRefreshSuggestions();
            }
        }

        private bool IsSuggestionsOpen = false;

        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public DateTime TrackingStarted { get; set; } = DateTime.Today;
        public DateTime TrackingStopped { get; set; }
        public bool IsBreak { get; set; }

        public double SuggestionsHeight
        {
            get
            {
                if (!IsSuggestionsOpen)
                    return 0;

                if (TrackedTimesViewSource.View is null || string.IsNullOrEmpty(TrackTimeTitle) || TrackedTimesViewSource.View.Cast<object>().Count() == 0)
                    return 0;

                return 165;
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

        public RelayCommand EnterButtonCommand { get; set; }
        public RelayCommand TabButtonCommand { get; set; }
        public RelayCommand EscButtonCommand { get; set; }
        public RelayCommand UpButtonCommand { get; set; }
        public RelayCommand DownButtonCommand { get; set; }
        public RelayCommand SaveAndCloseCommand { get; set; }
        public RelayCommand CancelAndCloseCommand { get; set; }

        private CancellationTokenSource refreshCancellationToken = new();

        public TrackTimeEditor(TrackTime? trackTime = null)
        {
            InitializeComponent();
            DataContext = this;

            if (trackTime is not null)
                TrackTime = trackTime;

            SaveAndCloseCommand = new((_) => SaveAndClose());
            CancelAndCloseCommand = new((_) => CancelAndClose());

            Loaded += async (s, e) =>
            {
                TrackedTimesViewSource = new();
                TrackedTimesViewSource.SetCurrentValue(CollectionViewSource.SourceProperty,
                    (await App.dbContext.TrackedTimes.OrderByDescending(t => t.Id).AsNoTracking()
                        .Take(SettingsModel.Instance.SearchNumberOfItems).ToListAsync())
                            .Select(t => new TitleMatch() { Title = t.Title }));

                TrackedTimesViewSource.Filter += TrackedTimesViewSource_Filter;
                TrackedTimesViewSource.SortDescriptions.Add(new("MatchPercentage", ListSortDirection.Descending));

                tbx_title.Focus();
                tbx_title.SelectAll();
            };

            EnterButtonCommand = new((_) =>
            {
                IsSuggestionsOpen = false;
                NotifyPropertyChanged(nameof(SuggestionsHeight));

                if (TrackedTimesViewSource.View.Cast<object>().Count() == 0)
                {
                    TitleMoveNext();
                    return;
                }

                string newTitle = TrackedTimesViewSource.View.Cast<TitleMatch>().ElementAt(SuggestionSelectedIndex).Title;

                if (TrackTimeTitle == newTitle)
                {
                    TitleMoveNext();
                    return;
                }

                TrackTimeTitle = newTitle;
                IsSuggestionsOpen = false;
                NotifyPropertyChanged(nameof(SuggestionsHeight));
            });

            TabButtonCommand = new((_) =>
            {
                IsSuggestionsOpen = false;
                NotifyPropertyChanged(nameof(SuggestionsHeight));
                TitleMoveNext();
            });

            EscButtonCommand = new((_) =>
            {
                TrackTimeTitle = TrackTime.Title;
                SaveAndClose();
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

        public TrackTimeEditor(DateTime startDate) : this()
        {
            SelectedDate = startDate;
        }

        private void TitleMoveNext()
        {
            TraversalRequest request = new(FocusNavigationDirection.Next) { Wrapped = true };
            tbx_title.MoveFocus(request);
        }

        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            TrackTime.Title = TrackTimeTitle;
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
                    titleMatch.CalculateMatchPercentage(TrackTimeTitle);

                TrackedTimesViewSource.SetCurrentValue(CollectionViewSource.SourceProperty, matches);
                TrackedTimesViewSource.View.Refresh();

                IsSuggestionsOpen = true;
                NotifyPropertyChanged(nameof(TrackedTimesViewSource));
                NotifyPropertyChanged(nameof(SuggestionsHeight));
            });
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrEmpty(TrackTimeTitle))
            {
                Close();
                return;
            }

            TrackTime.Title = TrackTimeTitle;
            TrackTime.TrackingStarted = SelectedDate.Date + TrackingStarted.TimeOfDay;
            TrackTime.StopTime(SelectedDate.Date + TrackingStopped.TimeOfDay);
            TrackTime.IsBreak = IsBreak;

            Close(TrackTime);
        }

        private void CancelAndClose()
        {
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SaveAndClose();
        }
    }
}
