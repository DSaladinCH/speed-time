using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DSaladin.TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private HotKeyManager hotKeyManager = new();
        //private HotKey openQuickTimeTracker;

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    openQuickTimeTracker = hotKeyManager.Register(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
        //    hotKeyManager.KeyPressed += HotKeyManagerPressed;
        //}

        //private void HotKeyManagerPressed(object? sender, KeyPressedEventArgs e)
        //{
        //    if (e.HotKey.Equals(openQuickTimeTracker))
        //    {
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            TrackTime? trackTime = QuickTimeTracker.Open(Application.Current.MainWindow);
        //            if (trackTime is null)
        //                return;

        //            //if (TrackedTimes.Count > 0)
        //            //    TrackedTimes.Last().StopTime();
        //            //TrackedTimes.Add(trackTime);
        //        });
        //    }
        //}
    }
}
