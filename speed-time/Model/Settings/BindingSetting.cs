using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal class BindingSetting<T>: BaseSetting
    {
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(T), typeof(BindingSetting<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }

    internal class CheckBoxSetting : BindingSetting<bool>
    {
    }

    internal class TextBoxSetting : BindingSetting<string>
    {
    }

    internal class NumberTextBoxSetting : BindingSetting<double>
    {
    }
}
