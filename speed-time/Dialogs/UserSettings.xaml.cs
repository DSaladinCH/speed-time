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

            CurrentVersion = FormatVersion(Assembly.GetExecutingAssembly().GetName().Version!);
        }

        private string FormatVersion(Version version)
        {
            int[] components = { version.Major, version.Minor, version.Build, version.Revision };
            int nonZeroIndex = components.Length - 1;

            while (nonZeroIndex > 1 && components[nonZeroIndex] == 0)
                nonZeroIndex--;

            return string.Join('.', components.Take(nonZeroIndex + 1));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsModel.Instance.SelectedUiLanguage))
            {
                Close(false);
                return;
            }

            Close(((App)Application.Current).CurrentUiLanguage.Name != SettingsModel.Instance.SelectedUiLanguage);
        }

        private void QuickEntry_OnLoadHotKey(object sender, HotKeyArgs e)
        {
            RegisteredHotKey registeredHotKey = SettingsModel.Instance.GetRegisteredHotKey(RegisteredHotKey.HotKeyType.QuickEntry);
            e.SelectedKey = registeredHotKey.RegisteredKey;
            e.SelectedModifierKeys = registeredHotKey.RegisteredModifierKeys;
            e.IsValid = true;
        }

        private void QuickEntry_OnHotKeyChanged(object sender, HotKeyArgs e)
        {
            e.IsValid = ((App)Application.Current).RegisterQuickTimeHotkey(new(e.SelectedKey, e.SelectedModifierKeys));
        }

        private void AddEntry_OnLoadHotKey(object sender, HotKeyArgs e)
        {
            RegisteredHotKey registeredHotKey = SettingsModel.Instance.GetRegisteredHotKey(RegisteredHotKey.HotKeyType.NewEntry);
            e.SelectedKey = registeredHotKey.RegisteredKey;
            e.SelectedModifierKeys = registeredHotKey.RegisteredModifierKeys;
            e.IsValid = true;
        }

        private void AddEntry_OnHotKeyChanged(object sender, HotKeyArgs e)
        {
            e.IsValid = !SettingsModel.Instance.IsHotKeyUsed(RegisteredHotKey.HotKeyType.NewEntry, e.SelectedKey, e.SelectedModifierKeys);
            SettingsModel.Instance.AddRegisteredHotKey(RegisteredHotKey.HotKeyType.NewEntry, e.SelectedKey, e.SelectedModifierKeys);
        }
    }
}
