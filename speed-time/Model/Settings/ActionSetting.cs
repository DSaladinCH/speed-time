using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal class ActionSetting: BaseSetting
    {
        private string actionText = "";
        public string ActionText
        {
            get { return actionText; }
            set
            {
                actionText = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ActionCommand
        {
            get { return (ICommand)GetValue(ActionCommandProperty); }
            set
            {
                SetValue(ActionCommandProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ActionCommandProperty =
            DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(ActionSetting), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
