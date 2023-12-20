using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Converter;
using DSaladin.SpeedTime.Integrations;
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

        public RelayCommand JiraCommand { get; set; }
        public RelayCommand WorkdaysCommand { get; set; }

        public UserSettings()
        {
            InitializeComponent();
            DataContext = this;

            #region ListBox Grouping
            lsbOptions.Items.IsLiveGrouping = true;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lsbOptions.ItemsSource);
            PropertyGroupDescription groupDescription = new("Category", new TranslatedEnumConverter());
            view.GroupDescriptions.Add(groupDescription);
            #endregion

            #region Commands
            JiraCommand = new(async a => await ShowDialog(new JiraSettings()));
            WorkdaysCommand = new(async a => await ShowDialog(new Workdays()));
            #endregion

            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsModel.Instance.SelectedUiLanguage))
            {
                Close(false);
                return;
            }

            Close(SpeedTime.Language.SpeedTime.Culture.Name != SettingsModel.Instance.SelectedUiLanguage);
        }

        private void QuickEntry_OnHotKeyChanged(object sender, HotKeyArgs e)
        {
            e.IsValid = ((App)Application.Current).RegisterQuickTimeHotkey(new(e.SelectedKey, e.SelectedModifierKeys));
        }
    }
}
