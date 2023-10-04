using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for Workdays.xaml
    /// </summary>
    public partial class Workdays : DSDialogControl
    {
        private double mondayHours;
        private double tuesdayHours;
        private double sundayHours;
        private double saturdayHours;
        private double fridayHours;
        private double thursdayHours;
        private double wednesdayHours;

        public double MondayHours
        {
            get => mondayHours;
            set
            {
                mondayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double TuesdayHours
        {
            get => tuesdayHours;
            set
            {
                tuesdayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double WednesdayHours
        {
            get => wednesdayHours;
            set
            {
                wednesdayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double ThursdayHours
        {
            get => thursdayHours;
            set
            {
                thursdayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double FridayHours
        {
            get => fridayHours;
            set
            {
                fridayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double SaturdayHours
        {
            get => saturdayHours;
            set
            {
                saturdayHours = value;
                NotifyPropertyChanged();
            }
        }
        public double SundayHours
        {
            get => sundayHours;
            set
            {
                sundayHours = value;
                NotifyPropertyChanged();
            }
        }

        public string MondayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Monday); }
        public string TuesdayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Tuesday); }
        public string WednesdayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Wednesday); }
        public string ThursdayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Thursday); }
        public string FridayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Friday); }
        public string SaturdayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Saturday); }
        public string SundayTitle { get => SpeedTime.Language.SpeedTime.Culture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Sunday); }

        public double TotalWorkHours { get => MondayHours + TuesdayHours + WednesdayHours + ThursdayHours + FridayHours + SaturdayHours + SundayHours; }
        public string TotalWorkHoursDisplay { get => $"{SpeedTime.Language.SpeedTime.workdays_total_hours}: {TotalWorkHours:N2}h"; }

        public Workdays()
        {
            InitializeComponent();
            DataContext = this;

            MondayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Monday);
            TuesdayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Tuesday);
            WednesdayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Wednesday);
            ThursdayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Thursday);
            FridayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Friday);
            SaturdayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Saturday);
            SundayHours = SettingsModel.Instance.Workdays.GetWorkHours(DayOfWeek.Sunday);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(TotalWorkHoursDisplay))
                    NotifyPropertyChanged(nameof(TotalWorkHoursDisplay));
            };
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Monday, MondayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Tuesday, TuesdayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Wednesday, WednesdayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Thursday, ThursdayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Friday, FridayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Saturday, SaturdayHours);
            SettingsModel.Instance.Workdays.SetWorkHours(new(), new(), DayOfWeek.Sunday, SundayHours);

            Close();
        }
    }
}
