﻿using DSaladin.SpeedTime.Integrations;
using DSaladin.SpeedTime.Model.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DSaladin.SpeedTime.Model
{
    internal class SettingsModel : INotifyPropertyChanged
    {
        internal record UiLanguage(string Id, string Name);

        private static SettingsModel instance = new();
        public static SettingsModel Instance
        {
            get { return instance; }
            private set { instance = value; }
        }

        private bool autoAddBreak;
        public bool? AutoAddBreak
        {
            get { return autoAddBreak; }
            set
            {
                if (value is null)
                    autoAddBreak = false;
                else
                    autoAddBreak = value.Value;

                NotifyPropertyChanged();
            }
        }

        private bool showGapsBetweenTimes;
        public bool? ShowGapsBetweenTimes
        {
            get { return showGapsBetweenTimes; }
            set
            {
                if (value is null)
                    showGapsBetweenTimes = false;
                else
                    showGapsBetweenTimes = value.Value;

                NotifyPropertyChanged();
            }
        }

        private int searchNumberOfItems = 100;
        public int SearchNumberOfItems
        {
            get { return searchNumberOfItems; }
            set
            {
                searchNumberOfItems = value;
                NotifyPropertyChanged();
            }
        }

        public Workdays Workdays { get; set; } = new();

        public static List<UiLanguage> AvailableLanguages { get; } = new()
        {
            new("en-US", "English (USA)"),
            new("de-CH", "Deutsch (Schweiz)"),
            new("es-ES", "Español (España)")
        };

        private string selectedUiLanguage = "";
        public string SelectedUiLanguage
        {
            get { return selectedUiLanguage; }
            set
            {
                selectedUiLanguage = value;
                NotifyPropertyChanged();
            }
        }

        private string selectedDateLanguage = "";
        public string SelectedDateLanguage
        {
            get { return selectedDateLanguage; }
            set
            {
                selectedDateLanguage = value;
                NotifyPropertyChanged();
            }
        }

        private bool jiraZeroOnDelete;
        public bool JiraZeroOnDelete
        {
            get { return jiraZeroOnDelete; }
            set
            {
                jiraZeroOnDelete = value;
                NotifyPropertyChanged();
            }
        }

        private List<RegisteredHotKey>? registeredHotKeys;
        [JsonInclude]
        protected List<RegisteredHotKey>? RegisteredHotKeys
        {
            get { return registeredHotKeys; }
            set
            {
                registeredHotKeys = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool JiraIsEnabled { get => App.dbContext.UserCredentials.Any(uc => uc.ServiceType == ServiceType.Jira); }

        public static void Load(SettingsModel settings)
        {
            Instance = settings;

            if (!AvailableLanguages.Exists(l => l.Id == Instance.SelectedUiLanguage))
                Instance.SelectedUiLanguage = AvailableLanguages[0].Id;

            if (!AvailableLanguages.Exists(l => l.Id == Instance.SelectedDateLanguage))
                Instance.SelectedDateLanguage = AvailableLanguages[0].Id;

            if (Instance.RegisteredHotKeys is null)
            {
                Instance.RegisteredHotKeys = [];
                Instance.AddRegisteredHotKey(RegisteredHotKey.HotKeyType.QuickEntry, Key.T, ModifierKeys.Alt | ModifierKeys.Control);
                Instance.AddRegisteredHotKey(RegisteredHotKey.HotKeyType.NewEntry, Key.A, ModifierKeys.Control);
            }
        }

        /// <summary>
        /// Replaces the current hotkey for the given <see cref="RegisteredHotKey.HotKeyType"/>. This function does not check if the HotKey is available
        /// </summary>
        public void AddRegisteredHotKey(RegisteredHotKey.HotKeyType hotKeyType, Key key, ModifierKeys modifierKeys)
        {
            if (IsHotKeyUsed(hotKeyType, key, modifierKeys))
            {
                key = Key.None;
                modifierKeys = ModifierKeys.None;
            }

            RegisteredHotKeys ??= [];
            RegisteredHotKey? registeredHotKey = RegisteredHotKeys.FirstOrDefault(r => r.Type == hotKeyType);

            if (registeredHotKey is not null)
                RegisteredHotKeys.Remove(registeredHotKey);

            registeredHotKey = new(hotKeyType, key, modifierKeys);
            RegisteredHotKeys.Add(registeredHotKey);
        }

        public RegisteredHotKey GetRegisteredHotKey(RegisteredHotKey.HotKeyType hotKeyType)
        {
            RegisteredHotKeys ??= [];
            RegisteredHotKey? registeredHotKey = RegisteredHotKeys.FirstOrDefault(r => r.Type == hotKeyType);

            if (registeredHotKey is not null)
                return registeredHotKey;

            return new(hotKeyType, Key.None, ModifierKeys.None);
        }

        public bool IsHotKeyUsed(RegisteredHotKey.HotKeyType checkForHotKeyType, Key key, ModifierKeys modifierKeys)
        {
            if (key == Key.None || modifierKeys == ModifierKeys.None)
                return false;

            RegisteredHotKeys ??= [];
            RegisteredHotKey? registeredHotKey = RegisteredHotKeys.FirstOrDefault(r => r.RegisteredKey == key && r.RegisteredModifierKeys == modifierKeys);

            if (registeredHotKey is null)
                return false;

            return registeredHotKey.Type != checkForHotKeyType;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        internal void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
