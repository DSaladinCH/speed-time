using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Integrations
{
    internal class JiraDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetString() is null)
                return DateTime.MinValue;

            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            string formattedDateTime;

            if (value.Kind == DateTimeKind.Utc)
                formattedDateTime = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff") + "+0000";
            else
                formattedDateTime = value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz").Replace(":", "");

            writer.WriteStringValue(formattedDateTime);
        }
    }
}
