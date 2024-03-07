using DSaladin.FancyPotato.DSWindows;
using DSaladin.SpeedTime.Model;
using DSaladin.SpeedTime.ViewModel;
using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace DSaladin.SpeedTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DSWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();

            //DateTime startDate = new DateTime(2023, 1, 1);
            //DateTime endDate = new DateTime(2023, 12, 31);

            //int range = (endDate - startDate).Days;
            //Random rand = new Random();

            //for (int i = 0; i < 1000; i++)
            //{
            //    int randomDays = rand.Next(range);
            //    DateTime randomDate = startDate.AddDays(randomDays);

            //    TrackTime trackTime = new(randomDate, $"Test {i}", false);
            //    trackTime.StopTime(randomDate.AddHours(3));
            //    App.dbContext.TrackedTimes.Add(trackTime);

            //    Debug.WriteLine($"Adding {i}");
            //}

            //App.dbContext.SaveChanges();
        }
    }
}
