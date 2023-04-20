using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    internal class SettingsModel : INotifyPropertyChanged
    {
        private static SettingsModel instance = new();
        public static SettingsModel Instance
        {
            get { return instance; }
            private set { instance = value; }
        }

        public ObservableCollection<Tuple<DateTime, double>> SpecialWorkHours { get; set; } = new();

        private bool autoAddBreak;
        public bool? AutoAddBreak
        {
            get { return autoAddBreak; }
            set
            {
                if (value is null)
                    autoAddBreak = false;
                else
                    autoAddBreak = value.Value;

                NotifyPropertyChanged();
            }
        }

        private double weeklyWorkHours;
        public double WeeklyWorkHours
        {
            get { return weeklyWorkHours; }
            set
            {
                weeklyWorkHours = value;
                NotifyPropertyChanged();
            }
        }

        private string uiLanguage = "";
        public string UiLanguage
        {
            get { return uiLanguage; }
            set
            {
                uiLanguage = value;
                NotifyPropertyChanged();
            }
        }


        public static void Load(SettingsModel settings)
        {
            Instance = settings;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
