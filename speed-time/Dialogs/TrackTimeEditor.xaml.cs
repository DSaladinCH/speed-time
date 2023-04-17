using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using System;
using System.Collections.Generic;
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
                TrackTimeTitle = value.Title;
                SelectedDate = value.TrackingStarted;
                TrackingStarted = value.TrackingStarted;
                TrackingStopped = value.TrackingStopped;
                IsBreak = value.IsBreak;
                NotifyPropertyChanged("");
            }
        }

        public string TrackTimeTitle { get; set; } = "";
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public DateTime TrackingStarted { get; set; } = DateTime.Today;
        public DateTime TrackingStopped { get; set; }
        public bool IsBreak { get; set; }

        public RelayCommand SaveAndCloseCommand { get; set; }
        public RelayCommand CancelAndCloseCommand { get; set; }

        public TrackTimeEditor(TrackTime? trackTime = null)
        {
            InitializeComponent();
            DataContext = this;

            if (trackTime is not null)
                TrackTime = trackTime;

            SaveAndCloseCommand = new((_) => SaveAndClose());
            CancelAndCloseCommand = new((_) => CancelAndClose());

            Loaded += (s, e) =>
            {
                tbx_title.Focus();
                tbx_title.SelectAll();
            };
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
