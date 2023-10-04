using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
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
    /// Interaction logic for ApiLog.xaml
    /// </summary>
    public partial class ApiLog : DSDialogControl
    {
        private List<Model.ApiLogEntry> logs = new();
        public List<Model.ApiLogEntry> Logs
        {
            get { return logs; }
            set
            {
                logs = value;
                NotifyPropertyChanged();
            }
        }

        public ApiLog(List<Model.ApiLogEntry> logs)
        {
            InitializeComponent();
            DataContext = this;

            foreach (Model.ApiLogEntry log in logs)
            {
                if (!log.IsSuccess)
                    Logs.Add(log);
                else if (!Logs.Exists(l => l.Context == log.Context && l.IsSuccess == log.IsSuccess))
                    Logs.Add(log);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
