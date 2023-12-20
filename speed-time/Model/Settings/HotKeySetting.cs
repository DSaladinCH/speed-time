using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using DSaladin.SpeedTime.Components;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal class HotKeySetting : BaseSetting
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

        public event EventHandler<HotKeyArgs>? OnHotKeyChanged;

        public HotKeySetting()
        {
            DataContext = this;
            this.AddHandler(HotKeySelector.HotKeyChangedEvent, new RoutedEventHandler(HotKeySelector_OnHotKeyChanged));
        }

        internal void HotKeySelector_OnHotKeyChanged(object sender, RoutedEventArgs e)
        {
            OnHotKeyChanged?.Invoke(sender, (HotKeyArgs)e);
        }
    }
}
