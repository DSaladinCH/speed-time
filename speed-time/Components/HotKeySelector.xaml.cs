using DSaladin.FancyPotato.DSUserControls;
using DSaladin.SpeedTime.Model;
using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSaladin.SpeedTime.Components
{
    /// <summary>
    /// Interaction logic for HotKeySelector.xaml
    /// </summary>
    public partial class HotKeySelector : DSUserControl
    {
        public Key SelectedKey
        {
            get => (Key)GetValue(SelectedKeyProperty);
            set => SetValue(SelectedKeyProperty, value);
        }

        public ModifierKeys SelectedModifierKeys
        {
            get => (ModifierKeys)GetValue(SelectedModifierKeysProperty);
            set => SetValue(SelectedModifierKeysProperty, value);
        }

        public string SelectedKeysText
        {
            get => (string)GetValue(SelectedKeysTextProperty);
            set => SetValue(SelectedKeysTextProperty, value);
        }

        public bool IsSelectingHotkey { get; set; } = false;

        public static readonly DependencyProperty SelectedKeyProperty = DependencyProperty.Register(nameof(SelectedKey), typeof(Key), typeof(HotKeySelector), new FrameworkPropertyMetadata(Key.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedKeyChanged));
        public static readonly DependencyProperty SelectedModifierKeysProperty = DependencyProperty.Register(nameof(SelectedModifierKeys), typeof(ModifierKeys), typeof(HotKeySelector), new FrameworkPropertyMetadata(ModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedModifierKeysChanged));
        public static readonly DependencyProperty SelectedKeysTextProperty = DependencyProperty.Register(nameof(SelectedKeysText), typeof(string), typeof(HotKeySelector), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private string selectedHotKey;
        public string SelectedHotKey
        {
            get { return selectedHotKey; }
            set
            {
                selectedHotKey = value;
                NotifyPropertyChanged();
            }
        }

        public static readonly RoutedEvent HotKeyChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(HotKeyChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HotKeySelector));

        public event RoutedEventHandler HotKeyChanged
        {
            add { AddHandler(HotKeyChangedEvent, value); }
            remove { RemoveHandler(HotKeyChangedEvent, value); }
        }

        public static readonly RoutedEvent LoadHotKeyEvent = EventManager.RegisterRoutedEvent(
            nameof(LoadHotKey), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HotKeySelector));

        public event RoutedEventHandler LoadHotKey
        {
            add { AddHandler(LoadHotKeyEvent, value); }
            remove { RemoveHandler(LoadHotKeyEvent, value); }
        }

        private Key[] modifierKeys = [Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.LWin, Key.RWin, Key.System];

        public HotKeySelector()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += HotKeySelector_Loaded;
        }

        private void HotKeySelector_Loaded(object sender, RoutedEventArgs e)
        {
            HotKeyArgs args = new(LoadHotKeyEvent, this, SelectedKey, SelectedModifierKeys);
            RaiseEvent(args);

            if (args.IsValid)
            {
                SetCurrentValue(SelectedKeyProperty, args.SelectedKey);
                SetCurrentValue(SelectedModifierKeysProperty, args.SelectedModifierKeys);
                ResetKeys(false);
            }
        }

        private static void OnSelectedKeyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            HotKeySelector hotkeySelector = (HotKeySelector)source;
            hotkeySelector.SetCurrentValue(SelectedKeyProperty, (Key)e.NewValue);
        }

        private static void OnSelectedModifierKeysChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            HotKeySelector hotkeySelector = (HotKeySelector)source;
            hotkeySelector.SetCurrentValue(SelectedModifierKeysProperty, (ModifierKeys)e.NewValue);
        }

        private static string GetKeyText(ModifierKeys modifierKeys, Key key)
        {
            string keyText = "";
            keyText += modifierKeys.HasFlag(ModifierKeys.Control) ? "Ctrl + " : "";
            keyText += modifierKeys.HasFlag(ModifierKeys.Alt) ? "Alt + " : "";
            keyText += modifierKeys.HasFlag(ModifierKeys.Shift) ? "Shift + " : "";
            keyText += key.ToString();

            return keyText;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
        }

        private void HotKey_Click(object sender, RoutedEventArgs e)
        {
            IsSelectingHotkey = !IsSelectingHotkey;

            if (IsSelectingHotkey)
                SetCurrentValue(SelectedKeysTextProperty, SpeedTime.Language.SpeedTime.hotkey_press_key);
            else
                ResetKeys(false);
        }

        private void HotKey_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetCurrentValue(SelectedKeyProperty, Key.None);
            SetCurrentValue(SelectedModifierKeysProperty, ModifierKeys.None);
            ResetKeys();
        }

        private void HotKey_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsSelectingHotkey)
            {
                Key currentKey = e.Key == Key.System ? e.SystemKey : e.Key;
                if (currentKey == Key.Escape)
                {
                    ResetKeys();
                    return;
                }

                if (currentKey == Key.None || Keyboard.Modifiers == ModifierKeys.None || modifierKeys.Contains(currentKey))
                    return;

                SetCurrentValue(SelectedKeyProperty, currentKey);
                SetCurrentValue(SelectedModifierKeysProperty, Keyboard.Modifiers);
                ResetKeys();
            }
        }

        private void ResetKeys(bool raiseChangedEvent = true)
        {
            IsSelectingHotkey = false;
            SetCurrentValue(SelectedKeysTextProperty, GetKeyText(SelectedModifierKeys, SelectedKey));

            if (raiseChangedEvent)
                RaiseKeyChangedEvent();
        }

        private void RaiseKeyChangedEvent()
        {
            HotKeyArgs args = new(HotKeyChangedEvent, this, SelectedKey, SelectedModifierKeys);
            RaiseEvent(args);
        }
    }
}
