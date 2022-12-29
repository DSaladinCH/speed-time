using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DSaladin.TimeTracker
{
    public class MainWindowViewModel
    {
        readonly HotKeyManager hotKeyManager = new();
        readonly HotKey openQuickTimeTracker;

        public MainWindowViewModel()
        {
            openQuickTimeTracker = hotKeyManager.Register(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.HotKey == openQuickTimeTracker)
                new QuickTimeTracker().ShowDialog();
        }
    }
}
