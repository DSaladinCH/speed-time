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
    public class DoubleConverter : DependencyObject, IValueConverter
    {
        public double SourceValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string)
                return value;

            SourceValue = (double)value;
            return ((double)value).ToString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The new value has to be saved in a variable, because it wont be saved otherwise
            if (!double.TryParse(value.ToString(), out double newValue))
                return SourceValue;

            return newValue;
        }
    }
}
