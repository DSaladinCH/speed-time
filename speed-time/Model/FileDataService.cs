using DSaladin.FancyPotato;
using DSaladin.SpeedTime.Dialogs;
using DSaladin.SpeedTime.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string CONFIG_NAME = "usersettings.json";

        public async Task LoadSettings()
        {
            SettingsModel? settings = null;
            try
            {
                FileStream stream = File.OpenRead(DSApplication.GetSettingsFilePath(CONFIG_NAME));
                settings = await JsonSerializer.DeserializeAsync<SettingsModel>(stream);
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while reading settings: " + ex.Message);
            }

            settings ??= new();
            SettingsModel.Load(settings);
        }

        public async Task SaveSettings()
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, SettingsModel.Instance);

            string filePath = DSApplication.GetSettingsFilePath(CONFIG_NAME);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            File.WriteAllText(filePath, Encoding.UTF8.GetString(stream.ToArray()));
            stream.Close();
        }
    }
}
