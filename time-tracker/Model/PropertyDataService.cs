using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker.Model
{
    internal class PropertyDataService : IDataService
    {
        public async Task LoadSettings()
        {
            string json = Properties.Settings.Default.AppSettings;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            SettingsModel? settings = null;
            try
            {
                settings = await System.Text.Json.JsonSerializer.DeserializeAsync<SettingsModel>(stream);
            }
            catch { }

            settings ??= new();

            SettingsModel.Load(settings);
        }

        public async Task SaveSettings()
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, SettingsModel.Instance);
            Properties.Settings.Default.AppSettings = Encoding.UTF8.GetString(stream.ToArray());
            Properties.Settings.Default.Save();
        }
    }
}
