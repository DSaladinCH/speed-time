using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DSaladin.SpeedTime.Converter
{
    public class DateTimeTimeConverter : DependencyObject, IValueConverter
    {
        public DateTime SourceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string)
                return value;

            SourceValue = (DateTime)value;
            return ((DateTime)value).ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string timeText = value.ToString()!;
            string hourText = "", minuteText = "";

            if (timeText.Contains(':'))
            {
                hourText = timeText[..timeText.IndexOf(":")];
                minuteText = timeText[(timeText.IndexOf(":") + 1)..];
            }
            else if (timeText.Length == 3)
            {
                hourText = timeText[..1];
                minuteText = timeText[1..];
            }
            else if (timeText.Length == 4)
            {
                hourText = timeText[..2];
                minuteText = timeText[2..];
            }

            if (!int.TryParse(hourText, out int hour) || !int.TryParse(minuteText, out int minute))
                return SourceValue;

            if (hour > 24 || minute >= 60)
                return SourceValue;

            // The new value has to be saved in a variable, because it wont be saved otherwise
            //DateTime newDateTime = SourceValue.Date + TimeSpan.Parse(value.ToString()!);
            DateTime newDateTime = SourceValue.Date + new TimeSpan(hour, minute, 0);
            return newDateTime;
        }
    }
}
