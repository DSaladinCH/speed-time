using DSaladin.FancyPotato.CustomControls;
using DSaladin.SpeedTime.Integrations;
using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace DSaladin.SpeedTime.Dialogs
{
    /// <summary>
    /// Interaction logic for JiraSettings.xaml
    /// </summary>
    public partial class JiraSettings : DSDialogControl
    {
        private string jiraEmail = "";
        public string JiraEmail
        {
            get { return jiraEmail; }
            set
            {
                jiraEmail = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(JiraApiToken));
            }
        }

        private string jiraBaseUrl = "";
        public string JiraBaseUrl
        {
            get { return jiraBaseUrl; }
            set
            {
                jiraBaseUrl = value;
                NotifyPropertyChanged();
            }
        }

        private string jiraApiToken = "";
        public string JiraApiToken
        {
            get { return jiraApiToken; }
            set
            {
                jiraApiToken = value;
                NotifyPropertyChanged();
            }
        }

        public JiraSettings()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (s, e) =>
            {
                UserCredential? userCredential = await App.dbContext.UserCredentials.FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);
                if (userCredential is not null)
                {
                    jiraBaseUrl = userCredential.ServiceUri;
                    NotifyPropertyChanged(nameof(JiraBaseUrl));
                }
            };

            PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == "")
                    return;

                await SaveAndClear();
                NotifyPropertyChanged("");
            };
        }

        private async Task SaveAndClear()
        {
            if (string.IsNullOrEmpty(JiraBaseUrl))
                return;

            UserCredential? userCredential = await App.dbContext.UserCredentials.AsNoTracking().FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);

            byte[] jiraToken = new byte[1];
            if (userCredential is not null)
                jiraToken = userCredential.Credential;

            if (!string.IsNullOrEmpty(JiraEmail) && !string.IsNullOrEmpty(JiraApiToken))
            {
                byte[] base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{JiraEmail}:{JiraApiToken}")));
                jiraApiToken = "";

                jiraToken = ProtectedData.Protect(base64, null, DataProtectionScope.CurrentUser);
            }

            EntityEntry<UserCredential>? trackedEntity = App.dbContext.ChangeTracker.Entries<UserCredential>()
                                    .FirstOrDefault(e => e.Entity.ServiceType == ServiceType.Jira);

            if (trackedEntity is not null)
                trackedEntity.State = EntityState.Detached;

            if (userCredential is null)
            {
                userCredential = new()
                {
                    ServiceType = ServiceType.Jira,
                    Credential = jiraToken,
                    ServiceUri = JiraBaseUrl
                };
                await App.dbContext.AddAsync(userCredential);
            }
            else
            {
                userCredential.Credential = jiraToken;
                userCredential.ServiceUri = JiraBaseUrl;
                App.dbContext.Update(userCredential);
            }

            await App.dbContext.SaveChangesAsync();
            SettingsModel.Instance.NotifyPropertyChanged(nameof(SettingsModel.Instance.JiraIsEnabled));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void DeleteClose_Click(object sender, RoutedEventArgs e)
        {
            UserCredential? userCredential = await App.dbContext.UserCredentials.FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);
            if (userCredential is not null)
            {
                App.dbContext.Remove(userCredential);
                await App.dbContext.SaveChangesAsync();
                SettingsModel.Instance.NotifyPropertyChanged(nameof(SettingsModel.Instance.JiraIsEnabled));
            }

            Close();
        }
    }
}
