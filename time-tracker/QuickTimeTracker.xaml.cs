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

namespace DSaladin.TimeTracker
{
    /// <summary>
    /// Interaction logic for QuickTimeTracker.xaml
    /// </summary>
    public partial class QuickTimeTracker : DSWindow
    {
        public QuickTimeTracker()
        {
            InitializeComponent();

            KeyUp += QuickTimeTracker_KeyUp;
        }

        private void QuickTimeTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                SaveAndClose();
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

        private void SaveAndClose()
        {
            throw new NotImplementedException();
        }
    }
}
