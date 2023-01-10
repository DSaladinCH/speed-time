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
        public async Task<List<TrackTime>> LoadTrackedTimes()
        {
            string json = Properties.Settings.Default.TrackedTimes;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            List<TrackTime>? trackedTimes = await System.Text.Json.JsonSerializer.DeserializeAsync<List<TrackTime>>(stream);

            if (trackedTimes is null)
                return new();

            return trackedTimes;
        }

        public async Task SaveTrackedTimes(List<TrackTime> trackedTimes)
        {
            var stream = new MemoryStream();
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, trackedTimes);
            Properties.Settings.Default.TrackedTimes = Encoding.UTF8.GetString(stream.ToArray());
            Properties.Settings.Default.Save();
        }
    }
}
