using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker.Model
{
    internal class SettingsModel
    {
        private static SettingsModel instance = new();
        public static SettingsModel Instance
        {
            get { return instance; }
            private set { instance = value; }
        }

        public ObservableCollection<Tuple<DateTime, double>> SpecialWorkHours { get; set; } = new();

        public static void Load(SettingsModel settings)
        {
            Instance = settings;
        }
    }
}
