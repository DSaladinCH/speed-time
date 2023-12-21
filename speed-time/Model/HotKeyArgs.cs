using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DSaladin.SpeedTime.Model
{
    public class HotKeyArgs: RoutedEventArgs
    {
        public Key SelectedKey { get; set; }
        public ModifierKeys SelectedModifierKeys { get; set; }
        public bool IsValid { get; set; }

        public HotKeyArgs(RoutedEvent routedEvent, object source, Key selectedKey, ModifierKeys selectedModifierKeys) : base(routedEvent, source)
        {
            SelectedKey = selectedKey;
            SelectedModifierKeys = selectedModifierKeys;
        }
    }
}
