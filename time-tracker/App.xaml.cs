using DSaladin.FontAwesome.WPF;
using DSaladin.TimeTracker.Model;
using GlobalHotKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
        internal static readonly TimeTrackerContext dbContext = new();
        private HotKeyManager hotKeyManager = new();
        private HotKey openQuickTimeTracker;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();

            openQuickTimeTracker = hotKeyManager.Register(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private async void HotKeyManagerPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Equals(openQuickTimeTracker))
            {
                TrackTime? trackTime = QuickTimeTracker.Open(await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync());
                if (trackTime is null)
                    return;

                // TODO: Add to AppModel
                TrackTime? lastTrackedTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();
                if (lastTrackedTime is not null)
                {
                    lastTrackedTime.StopTime();
                    dbContext.TrackedTimes.Update(lastTrackedTime);
                }

                await dbContext.TrackedTimes.AddAsync(trackTime);
                await dbContext.SaveChangesAsync();
            }
        }

        private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                //I left my desk
                Console.WriteLine("I left my desk");

                // TODO: Add settings to check if this option is enabled
                TrackTime? lastWorkTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();

                if (lastWorkTime is null)
                    return;

                lastWorkTime.StopTime();
                dbContext.TrackedTimes.Update(lastWorkTime);
                await dbContext.TrackedTimes.AddAsync(new(DateTime.Now, "Automatic pause detection", true));
                await dbContext.SaveChangesAsync();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                //I returned to my desk
                Console.WriteLine("I returned to my desk");

                // TODO: Add settings to check if this option is enabled
                TrackTime? lastWorkTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).Reverse().Skip(1).Take(1).FirstOrDefaultAsync();
                TrackTime? breakTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();

                if (lastWorkTime is null || breakTime is null)
                    return;
                
                breakTime.StopTime();
                dbContext.TrackedTimes.Update(breakTime);
                await dbContext.TrackedTimes.AddAsync(new(DateTime.Now, lastWorkTime.Title, lastWorkTime.IsBreak));
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
