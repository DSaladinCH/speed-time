using DSaladin.FancyPotato;
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
    /// Interaction logic for TrackTimeEditor.xaml
    /// </summary>
    public partial class TaskLinkEditor : DSDialogControl
    {
        private TaskLink taskLink = TaskLink.Empty();

        public TaskLink TaskLink
        {
            get { return taskLink; }
            set
            {
                taskLink = value;
                TaskCheck = value.TaskCheck;
                TaskLinkUri = value.TaskLinkUri;
                ContainsNumbers = value.ContainsNumbers;
                ContainsLetters = value.ContainsLetters;
                NotifyPropertyChanged("");
            }
        }

        public string TaskCheck { get; set; } = "";
        public string TaskLinkUri { get; set; } = "";
        public bool ContainsNumbers { get; set; }
        public bool ContainsLetters { get; set; }

        public RelayCommand SaveAndCloseCommand { get; set; }
        public RelayCommand CancelAndCloseCommand { get; set; }

        public TaskLinkEditor(TaskLink? taskLink = null)
        {
            InitializeComponent();
            DataContext = this;

            if (taskLink is not null)
                TaskLink = taskLink;

            SaveAndCloseCommand = new((_) => SaveAndClose());
            CancelAndCloseCommand = new((_) => CancelAndClose());

            Loaded += (s, e) =>
            {
                tbx_check.Focus();
                tbx_check.SelectAll();
            };
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrEmpty(TaskCheck) || string.IsNullOrEmpty(TaskLinkUri))
            {
                Close();
                return;
            }

            TaskLink.TaskCheck = TaskCheck;
            TaskLink.TaskLinkUri = TaskLinkUri;
            TaskLink.ContainsNumbers = ContainsNumbers;
            TaskLink.ContainsLetters = ContainsLetters;

            Close(TaskLink);
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
