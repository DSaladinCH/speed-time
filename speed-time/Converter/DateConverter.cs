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
    public class DateConverter : IValueConverter
    {
        public DateTime SourceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SourceValue = (DateTime)value;
            return SourceValue.ToString("dd.MM.yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string dateString)
                return SourceValue;

            DateTime currentDate = DateTime.Now;
            string[] formats = {
                    "dd.MM.yyyy",
                    "dd.MM.yy",
                    "ddMMyyyy",
                    "ddMMyy",
                    "dd.MM",
                    "ddMM",
                    "d",
                    "dd"
                };

            // Add zero, if the value is single character date
            if (dateString.Length == 1)
                dateString = "0" + dateString;

            DateTime parsedDate;
            try
            {
                if (!DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    return SourceValue;
            }
            catch (FormatException)
            {
                return SourceValue;
            }

            if (parsedDate.Year == 1)
                parsedDate = parsedDate.AddYears(currentDate.Year - 1);

            if (parsedDate.Month == 1 && !formats[1].Equals(dateString) && !formats[2].Equals(dateString))
                parsedDate = parsedDate.AddMonths(currentDate.Month - 1);

            return parsedDate;

        }
    }
}
