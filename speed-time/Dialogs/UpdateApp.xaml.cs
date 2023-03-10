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

        public UpdateApp()
        {
            InitializeComponent();
            DataContext = this;

            DenyCommand = new((_) => Close(false));
            DownloadCommand = new((_) => Close(true));
        }
    }
}
