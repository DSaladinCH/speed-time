using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSaladin.SpeedTime.Converter
{
    internal class ListToGenericListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the value is already a List<object>, just return it
            if (value is List<object> objList)
            {
                return objList;
            }

            // Otherwise, try to convert any IEnumerable to List<object>
            return value is IEnumerable enumerable ? enumerable.Cast<object>().ToList() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
