using DSaladin.SpeedTime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Integrations
{
    public class Worklog
    {
        [JsonPropertyName("comment")]
        public Comment Comment { get; set; }

        [JsonPropertyName("started")]
        public string Started { get; set; }

        [JsonPropertyName("timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        public Worklog(TrackTime trackTime, bool zeroDuration = false)
        {
            Comment = new Comment(trackTime.Title.Trim());
            TimeSpentSeconds = 0;

            if (!zeroDuration)
            {
                Started = trackTime.TrackingStarted.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz");
                Started = Started.Remove(Started.LastIndexOf(':'), 1);
                TimeSpentSeconds = Convert.ToInt32((trackTime.TrackingStopped - trackTime.TrackingStarted).TotalSeconds);
            }
        }
    }

    public class Comment
    {
        [JsonPropertyName("content")]
        public List<ContentItem> Content { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "doc";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        public Comment(string comment)
        {
            ContentItem innerContent = new()
            {
                Text = comment
            };

            ContentItem outerContent = new()
            {
                Type = "paragraph",
                Content = new() { innerContent }
            };

            Content.Add(outerContent);
        }
    }

    public class ContentItem
    {
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ContentItem>? Content { get; set; }

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";
    }
}
