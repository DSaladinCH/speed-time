using DSaladin.FancyPotato;
using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for TaskLinking.xaml
    /// </summary>
    public partial class TaskLinking : DSDialogControl
    {
        public RelayCommand AddCommand { get; set; }
        public RelayCommand TaskLinkDoubleClickCommand { get; set; }
        public RelayCommand TaskLinkDeleteCommand { get; set; }

        private bool isEditOpen = false;

        public ObservableCollection<TaskLink> TaskLinks { get; set; } = new();

        public TaskLinking(List<TaskLink> taskLinks)
        {
            InitializeComponent();
            DataContext = this;
            TaskLinks = new(taskLinks);

            AddCommand = new(async sender =>
            {
                if (isEditOpen)
                    return;

                isEditOpen = true;
                TaskLink? taskLink = await ShowDialog<TaskLink>(new TaskLinkEditor());
                isEditOpen = false;

                if (taskLink is not null)
                    TaskLinks.Add(taskLink);

                NotifyPropertyChanged("");
            });

            TaskLinkDoubleClickCommand = new(async sender =>
            {
                if (isEditOpen)
                    return;

                isEditOpen = true;
                await ShowDialog(new TaskLinkEditor((TaskLink)sender));
                isEditOpen = false;

                NotifyPropertyChanged("");
            });

            TaskLinkDeleteCommand = new(sender =>
            {
                TaskLinks.Remove((TaskLink)sender);
                NotifyPropertyChanged("");
            });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close(TaskLinks.ToList());
        }
    }
}
