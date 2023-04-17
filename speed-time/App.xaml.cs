using DSaladin.FancyPotato.DSWindows;
using DSaladin.FontAwesome.WPF;
using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Model;
using GlobalHotKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DSaladin.SpeedTime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal const string ProductId = "61dd9fcc-ba93-406f-89b9-99763fd2077c";

        internal static readonly TimeTrackerContext dbContext = new();
        internal static readonly IDataService DataService = new FileDataService();

        private readonly HotKeyManager hotKeyManager = new();
        private HotKey? openQuickTimeTracker;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();
            await DataService.LoadSettings();

            TrackTime? lastTrackedTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();
            if (lastTrackedTime is not null && !lastTrackedTime.IsTimeStopped)
            {
                if (lastTrackedTime.TrackingStarted.Date < DateTime.Today)
                {
                    lastTrackedTime.StopTime();
                    dbContext.TrackedTimes.Update(lastTrackedTime);
                    await dbContext.SaveChangesAsync();
                }
            }

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

                TrackTime? lastTrackedTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();
                if (lastTrackedTime is not null && !lastTrackedTime.IsTimeStopped)
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
            if (SettingsModel.Instance.AutoAddBreak != true)
                return;

            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                TrackTime? lastWorkTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();

                if (lastWorkTime is null)
                    return;

                if (lastWorkTime.IsAFK)
                    return;

                lastWorkTime.StopTime();
                dbContext.TrackedTimes.Update(lastWorkTime);
                await dbContext.TrackedTimes.AddAsync(new(DateTime.Now, "Automatic pause detection", true));
                await dbContext.SaveChangesAsync();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                // TODO: Add settings to check if this option is enabled
                TrackTime? lastWorkTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).Reverse().Skip(1).Take(1).FirstOrDefaultAsync();
                TrackTime? breakTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();

                if (lastWorkTime is null || breakTime is null || !breakTime.IsBreak)
                    return;

                breakTime.StopTime();
                dbContext.TrackedTimes.Update(breakTime);
                await dbContext.TrackedTimes.AddAsync(new(DateTime.Now, lastWorkTime.Title, lastWorkTime.IsBreak));
                await dbContext.SaveChangesAsync();
            }
        }

        async void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            await StopTimeAndSave();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            await StopTimeAndSave();
            await DataService.SaveSettings();
        }

        private static async Task StopTimeAndSave()
        {
            TrackTime? lastWorkTime = await dbContext.TrackedTimes.OrderBy(tt => tt.Id).LastOrDefaultAsync();

            if (lastWorkTime is null || lastWorkTime.IsTimeStopped)
                return;

            lastWorkTime.StopTime();
            dbContext.TrackedTimes.Update(lastWorkTime);
            await dbContext.SaveChangesAsync();
        }

        internal static async Task CheckForUpdate()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string exeFile = Path.Combine(assemblyDirectory, "Downloads/setup.exe");

            if (File.Exists(exeFile))
                File.Delete(exeFile);

            HttpRequestMessage requestMessage = new(HttpMethod.Get, $"https://api.dsaladin.dev/v1/product/{ProductId}/versions?fromVersion={Assembly.GetExecutingAssembly().GetName().Version!}");
            HttpResponseMessage responseMessage = await new HttpClient().SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
                return;

            List<object>? versions = await System.Text.Json.JsonSerializer.DeserializeAsync<List<object>>(responseMessage.Content.ReadAsStream());
            if (versions is null || versions.Count == 0)
                return;

            bool result = await Current.Dispatcher.Invoke(async () =>
            {
                return await (Current.MainWindow as DSWindow)!.ShowDialog<bool>(new UpdateApp());
            });

            if (result == false)
                return;

            HttpResponseMessage exeResponseMessage = await new HttpClient().SendAsync(new(HttpMethod.Get, $"https://api.dsaladin.dev/v1/product/{ProductId}/latest"));
            if (!exeResponseMessage.IsSuccessStatusCode)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(exeFile)!);
            using var fileStream = new FileStream(exeFile, FileMode.CreateNew);
            await exeResponseMessage.Content.CopyToAsync(fileStream);
            fileStream.Close();

            Current.Dispatcher.Invoke(() =>
            {
                Process.Start(exeFile);
                Current.Shutdown(0);
            });
        }
    }
}
