using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DSaladin.SpeedTime.Model
{
    public class RegisteredHotKey
    {
        public HotKeyType Type { get; set; }
        public Key RegisteredKey { get; set; }
        public ModifierKeys RegisteredModifierKeys { get; set; }

        public RegisteredHotKey(HotKeyType type, Key registeredKey, ModifierKeys registeredModifierKeys)
        {
            Type = type;
            RegisteredKey = registeredKey;
            RegisteredModifierKeys = registeredModifierKeys;
        }

        public HotKey GetGlobalHotKey()
        {
            return new(RegisteredKey, RegisteredModifierKeys);
        }

        public enum HotKeyType
        {
            QuickEntry,
            NewEntry
        }
    }
}
