using DSaladin.SpeedTime.Model.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DSaladin.SpeedTime.Converter
{
    internal class TranslatedEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetEnumDescription((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo? fi = value.GetType().GetField(value.ToString());

            if (fi is not null && fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
                return Language.SpeedTime.ResourceManager.GetString(attributes.First().Description, ((App)Application.Current).CurrentUiLanguage) ?? value.ToString();

            return value.ToString();
        }
    }
}
