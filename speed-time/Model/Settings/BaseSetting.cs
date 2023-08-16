using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal abstract class BaseSetting : Control, INotifyPropertyChanged
    {
        private SettingCategory category;
        public SettingCategory Category
        {
            get { return category; }
            set
            {
                category = value;
                NotifyPropertyChanged();
            }
        }

        private string optionName = "";
        public string OptionName
        {
            get { return optionName; }
            set
            {
                optionName = value.ToUpper();
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
