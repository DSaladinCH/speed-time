using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DSaladin.SpeedTime.Converter
{
    public class DateTimeConverter : DependencyObject, IValueConverter
    {
        public DateTime SourceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string)
                return value;

            SourceValue = (DateTime)value;
            return ((App)Application.Current).FormatDate((DateTime)value, (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The new value has to be saved in a variable, because it wont be saved otherwise
            DateTime newDateTime = SourceValue.Date + TimeSpan.Parse(value.ToString()!);
            return newDateTime;
        }
    }
}
