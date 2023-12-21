using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.ViewModel;
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
    /// Interaction logic for UpdateApp.xaml
    /// </summary>
    public partial class UpdateApp : DSDialogControl
    {
        public RelayCommand DenyCommand { get; set; }
        public RelayCommand DownloadCommand { get; set; }

        private string version;
        public string Version
        {
            get { return version; }
            set
            {
                version = value;
                NotifyPropertyChanged();
            }
        }

        private string releaseDate;
        public string ReleaseDate
        {
            get { return releaseDate; }
            set
            {
                releaseDate = value;
                NotifyPropertyChanged();
            }
        }

        public UpdateApp(Version version, DateTime releaseDate)
        {
            InitializeComponent();
            DataContext = this;

            int[] components = { version.Major, version.Minor, version.Build, version.Revision };
            int nonZeroIndex = components.Length - 1;

            while (nonZeroIndex > 1 && components[nonZeroIndex] <= 0)
                nonZeroIndex--;

            Version = string.Join('.', components.Take(nonZeroIndex + 1));

            if (releaseDate.Year == DateTime.Now.Year)
                ReleaseDate = releaseDate.ToString("M", SpeedTime.Language.SpeedTime.Culture);
            else
                ReleaseDate = releaseDate.ToString("D", SpeedTime.Language.SpeedTime.Culture);

            DenyCommand = new((_) => Close(false));
            DownloadCommand = new((_) => Close(true));
        }
    }
}
