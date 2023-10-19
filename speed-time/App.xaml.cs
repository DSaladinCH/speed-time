using DSaladin.FancyPotato.DSWindows;
using DSaladin.FontAwesome.WPF;
using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Model;
using GlobalHotKey;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace DSaladin.SpeedTime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        record AppVersion(string Version, DateTime ReleaseDate);

        internal const string ProductId = "61dd9fcc-ba93-406f-89b9-99763fd2077c";

        internal static readonly TimeTrackerContext dbContext = new();
        internal static readonly IDataService DataService = new FileDataService();

        private readonly HotKeyManager hotKeyManager = new();
        private HotKey? openQuickTimeTracker;

        private TrackTime? trackTimeBeforePause;
        private TrackTime? trackTimePause;

        protected override async void OnStartup(StartupEventArgs e)
        {
            await dbContext.Database.MigrateAsync();
            await DataService.LoadSettings();

            if (string.IsNullOrEmpty(SettingsModel.Instance.SelectedUiLanguage))
                Language.SpeedTime.Culture = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name);
            else
                Language.SpeedTime.Culture = new CultureInfo(SettingsModel.Instance.SelectedUiLanguage);

            Thread.CurrentThread.CurrentCulture = Language.SpeedTime.Culture;
            Thread.CurrentThread.CurrentUICulture = Language.SpeedTime.Culture;

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

            base.OnStartup(e);

            LiveCharts.Configure(config =>
            {
                config.AddDarkTheme();
            });

            MainWindow mainWindow = new();
            mainWindow.Show();
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
                trackTimeBeforePause = await dbContext.TrackedTimes.OrderBy(t => t.Id).LastOrDefaultAsync(t => t.TrackingStopped == default);

                if (trackTimeBeforePause is not null)
                {
                    if (trackTimeBeforePause.IsAFK || trackTimeBeforePause.IsBreak)
                        return;

                    trackTimeBeforePause.StopTime();
                    dbContext.TrackedTimes.Update(trackTimeBeforePause);
                }

                trackTimePause = new(DateTime.Now, Language.SpeedTime.automatic_pause, true);
                await dbContext.TrackedTimes.AddAsync(trackTimePause);
                await dbContext.SaveChangesAsync();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                if (trackTimePause is null || !trackTimePause.IsBreak)
                    return;

                trackTimePause.StopTime();
                dbContext.TrackedTimes.Update(trackTimePause);

                if (trackTimeBeforePause is not null)
                    await dbContext.TrackedTimes.AddAsync(new(DateTime.Now, trackTimeBeforePause.Title, trackTimeBeforePause.IsBreak));

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

            HttpResponseMessage responseMessage;
            try
            {
                responseMessage = await new HttpClient().SendAsync(requestMessage);
                if (!responseMessage.IsSuccessStatusCode)
                    return;
            }
            catch { return; }

            List<AppVersion>? versions = await System.Text.Json.JsonSerializer.DeserializeAsync<List<AppVersion>>(responseMessage.Content.ReadAsStream());
            if (versions is null || versions.Count == 0)
                return;

            bool result = await Current.Dispatcher.Invoke(async () =>
            {
                return await (Current.MainWindow as DSWindow)!.ShowDialog<bool>(new UpdateApp(versions.Last().Version, versions.Last().ReleaseDate));
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
                Process.Start(exeFile, "/silent");
                Current.Shutdown(0);
            });
        }
    }
}
