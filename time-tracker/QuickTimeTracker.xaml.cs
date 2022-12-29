using DSaladin.FancyPotato.DSWindows;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DSaladin.TimeTracker
{
    /// <summary>
    /// Interaction logic for QuickTimeTracker.xaml
    /// </summary>
    public partial class QuickTimeTracker : DSWindow
    {
        private string workTitle;
        public string WorkTitle
        {
            get { return workTitle; }
            set
            {
                workTitle = value;
                NotifyPropertyChanged();
            }
        }

        private bool? isBreak = false;
        public bool? IsBreak
        {
            get { return isBreak; }
            set
            {
                isBreak = value;
                NotifyPropertyChanged();
            }
        }

        private QuickTimeTracker()
        {
            InitializeComponent();

            KeyUp += QuickTimeTracker_KeyUp;
        }

        public static TrackTime? Open(Window parent)
        {
            return parent.Dispatcher.Invoke<TrackTime?>(() =>
            {
                QuickTimeTracker quickTimeTracker = new();
                quickTimeTracker.Focus();
                quickTimeTracker.ShowDialog();

                if (quickTimeTracker.WorkTitle == "")
                    return null;

                return new(DateTime.Now, quickTimeTracker.WorkTitle, quickTimeTracker.IsBreak != null && quickTimeTracker.IsBreak != false);
            });
        }

        private void QuickTimeTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WorkTitle = "";
                Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                Close();
                return;
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.B)
                {
                    ckb_break.IsChecked = !ckb_break.IsChecked;
                    return;
                }
            }
        }
    }
}
