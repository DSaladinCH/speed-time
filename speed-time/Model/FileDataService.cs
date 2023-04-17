using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    internal class FileDataService : IDataService
    {
        private const string PUBLISHER = "DSaladin";
        private const string APPNAME = "speed-time";
        private const string FILENAME = "usersettings.json";

        public async Task LoadSettings()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(appDataPath, PUBLISHER, APPNAME, FILENAME);

            SettingsModel? settings = null;
            try
            {
                FileStream stream = File.OpenRead(filePath);
                settings = await JsonSerializer.DeserializeAsync<SettingsModel>(stream);
            }
            catch { }

            settings ??= new();
            SettingsModel.Load(settings);
        }

        public async Task SaveSettings()
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, SettingsModel.Instance);

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderPath = Path.Combine(appDataPath, PUBLISHER, APPNAME);
            Directory.CreateDirectory(appFolderPath);

            File.WriteAllText(Path.Combine(appFolderPath, FILENAME), Encoding.UTF8.GetString(stream.ToArray()));
        }
    }
}
