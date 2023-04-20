using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : DSDialogControl
    {
        private string currentVersion;
        public string CurrentVersion
        {
            get { return currentVersion; }
            set
            {
                currentVersion = value;
                NotifyPropertyChanged();
            }
        }

        public UserSettings()
        {
            InitializeComponent();
            DataContext = this;

            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsModel.Instance.UiLanguage))
            {
                Close(false);
                return;
            }

            Close(SpeedTime.Language.SpeedTime.Culture.Name != SettingsModel.Instance.UiLanguage);
        }
    }
}
