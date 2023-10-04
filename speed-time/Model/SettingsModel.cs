using DSaladin.SpeedTime.Integrations;
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

        public ObservableCollection<Tuple<DateTime, double>> SpecialWorkHours { get; set; } = new();

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

        private List<TaskLink> taskLinks = new();
        public List<TaskLink> TaskLinks
        {
            get { return taskLinks; }
            set
            {
                taskLinks = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(TaskLinkCollection));
            }
        }

        public ObservableCollection<TaskLink> TaskLinkCollection
        {
            get { return new(taskLinks); }
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

        [JsonIgnore]
        public bool JiraIsEnabled { get => App.dbContext.UserCredentials.Any(uc => uc.ServiceType == ServiceType.Jira); }

        public static void Load(SettingsModel settings)
        {
            Instance = settings;

            if (!AvailableLanguages.Exists(l => l.Id == Instance.SelectedUiLanguage))
                Instance.SelectedUiLanguage = AvailableLanguages[0].Id;

            Instance.TaskLinkCollection.CollectionChanged += (s, e) =>
            {
                if (e.NewItems is not null)
                    Instance.TaskLinks.AddRange(e.NewItems.Cast<TaskLink>());

                if (e.OldItems is not null)
                    foreach (TaskLink taskLink in e.OldItems.Cast<TaskLink>())
                        Instance.TaskLinks.Remove(taskLink);
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        internal void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
