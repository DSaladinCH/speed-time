using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Integrations
{
    internal class JiraService
    {
        internal void CreateWorklog()
        {

        }

        internal void UpdateWorklog()
        {

        }

        internal static async Task GetNewJiraTokenAsync()
        {
            byte[] jiraToken = ProtectedData.Protect(Encoding.UTF8.GetBytes(FancyPotato.Dialog.InputBox.ShowDialog("Jira Key")), null, DataProtectionScope.CurrentUser);
            string serviceUri = FancyPotato.Dialog.InputBox.ShowDialog("Jira Url");
            UserCredential? userCredential = await App.dbContext.UserCredentials.FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);

            if (userCredential is null)
            {
                userCredential = new()
                {
                    ServiceType = ServiceType.Jira,
                    Credential = jiraToken,
                    ServiceUri = serviceUri
                };
                await App.dbContext.AddAsync(userCredential);
            }
            else
            {
                userCredential.Credential = jiraToken;
                userCredential.ServiceUri = serviceUri;
                App.dbContext.Update(userCredential);
            }

            await App.dbContext.SaveChangesAsync();
        }
    }
}
