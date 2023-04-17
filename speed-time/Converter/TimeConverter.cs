using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DSaladin.SpeedTime.Converter
{
    public class TimeConverter : IValueConverter
    {
        public DateTime SourceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SourceValue = (DateTime)value;
            return SourceValue.ToString("HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string timeString)
                return SourceValue;

            DateTime currentTime = DateTime.Now;
            string[] formats = {
                "HHmm",
                "HH:mm",
                "Hmm",
                "H:mm",
                "HH",
                "H"
            };

            // Add zero, if the value is single character date
            if (timeString.Length == 1)
                timeString += ":00";

            // Add zero, if the value is Hmm character date
            if (timeString.Length == 3)
                timeString = "0" + timeString;

            DateTime parsedTime;
            try
            {
                if (!DateTime.TryParseExact(timeString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                    return SourceValue;
            }
            catch (FormatException)
            {
                return SourceValue;
            }

            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, parsedTime.Hour, parsedTime.Minute, 0);

        }
    }
}
