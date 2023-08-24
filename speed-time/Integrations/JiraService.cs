using DSaladin.FontAwesome.WPF;
using DSaladin.SpeedTime.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace DSaladin.SpeedTime.Integrations
{
    // TODO: Handle renaming TrackTime -> Check if Jira Key changed
    internal partial class JiraService
    {
        [GeneratedRegex("(?:^| )([A-z]+-[0-9]+)")]
        private static partial Regex JiraIssueKeyRegex();

        private const string IssueKeyAttribute = "JIRAISSUEKEY";
        private const string WorklogAttribute = "JIRAWORKLOGID";

        internal static async Task UploadWorklogsAsync(List<TrackTime> trackTimes)
        {
            await UpdateWorklogsAsync(trackTimes);
        }

        private static async Task UpdateWorklogsAsync(List<TrackTime> trackTimes)
        {
            if (trackTimes.Count == 0)
                return;

            UserCredential? userCredential = await App.dbContext.UserCredentials.FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);
            if (userCredential is null)
                return;

            byte[] decryptedBasicAuthentication = new byte[1];
            string? basicAuthentication = null;
            try
            {
                using var client = new HttpClient();
                decryptedBasicAuthentication = ProtectedData.Unprotect(userCredential.Credential, null, DataProtectionScope.CurrentUser);
                basicAuthentication = Encoding.UTF8.GetString(decryptedBasicAuthentication);
                client.BaseAddress = new Uri(userCredential.ServiceUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

                foreach (TrackTime trackTime in trackTimes)
                {
                    bool shouldCreate = false;
                    bool shouldDelete = false;

                    string? savedKey = trackTime.GetAttribute(IssueKeyAttribute);
                    string? currentIssueKey = GetIssueKey(trackTime);

                    if (savedKey is null && currentIssueKey is null)
                        continue;

                    if (savedKey is null && currentIssueKey is not null)
                        shouldCreate = true;

                    if (savedKey is not null && currentIssueKey is null)
                        shouldDelete = true;

                    if (savedKey is not null && currentIssueKey is not null)
                        if (savedKey != currentIssueKey)
                        {
                            shouldCreate = true;
                            shouldDelete = true;
                        }

                    if (shouldDelete)
                        // TODO: Show error
                        if (!await DeleteWorklogAsync(trackTime))
                            continue;

                    HttpRequestMessage? httpRequestMessage = null;
                    if (!shouldCreate && !shouldDelete)
                    {
                        string? worklogId = trackTime.GetAttribute(WorklogAttribute);
                        if (worklogId is null)
                            continue;

                        httpRequestMessage = new(HttpMethod.Put, $"/rest/api/3/issue/{currentIssueKey}/worklog/{worklogId}");
                    }
                    else if (shouldCreate)
                        httpRequestMessage = new(HttpMethod.Post, $"/rest/api/3/issue/{currentIssueKey}/worklog");

                    // No request should be sent
                    if (httpRequestMessage is null)
                        continue;

                    httpRequestMessage.Content = await GetContentAsync(trackTime);

                    HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    using JsonDocument? response = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStreamAsync());

                    if (response is null)
                        continue;

                    // TODO: Show error
                    if (!httpResponseMessage.IsSuccessStatusCode)
                        continue;

                    trackTime.SetAttribute(IssueKeyAttribute, currentIssueKey!);

                    if (response.RootElement.TryGetProperty("id", out JsonElement id))
                        trackTime.SetAttribute(WorklogAttribute, id.GetString()!);
                }
            }
            finally
            {
                // Zero out the char array
                Array.Clear(decryptedBasicAuthentication);

                // If you also want to attempt to zero out the string's underlying memory (though with limitations due to string immutability):
                if (basicAuthentication != null)
                {
                    GCHandle handle = GCHandle.Alloc(basicAuthentication, GCHandleType.Pinned);
                    for (int i = 0; i < basicAuthentication.Length; i++)
                    {
                        Marshal.WriteByte(handle.AddrOfPinnedObject() + i * 2, 0);
                    }
                    handle.Free();
                }
            }
        }

        internal static async Task<bool> DeleteWorklogAsync(TrackTime trackTime)
        {
            if (!trackTime.ContainsAttribute(WorklogAttribute))
                return true;

            UserCredential? userCredential = await App.dbContext.UserCredentials.FirstOrDefaultAsync(uc => uc.ServiceType == ServiceType.Jira);
            if (userCredential is null)
                return false;

            byte[] decryptedBasicAuthentication = new byte[1];
            string? basicAuthentication = null;
            try
            {
                using var client = new HttpClient();
                decryptedBasicAuthentication = ProtectedData.Unprotect(userCredential.Credential, null, DataProtectionScope.CurrentUser);
                basicAuthentication = Encoding.UTF8.GetString(decryptedBasicAuthentication);
                client.BaseAddress = new Uri(userCredential.ServiceUri);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthentication);

                string? issueKey = trackTime.GetAttribute(IssueKeyAttribute);
                if (issueKey is null)
                    return true;

                string? worklogId = trackTime.GetAttribute(WorklogAttribute);
                if (worklogId is null)
                    return true;

                HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"/rest/api/3/issue/{issueKey}/worklog/{worklogId}");
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                string response = await httpResponseMessage.Content.ReadAsStringAsync();

                // If the remove was successful, remove the attributes
                if (httpResponseMessage.IsSuccessStatusCode)
                    trackTime.RemoveAttributes(IssueKeyAttribute, WorklogAttribute);

                return httpResponseMessage.IsSuccessStatusCode;
            }
            finally
            {
                // Zero out the char array
                Array.Clear(decryptedBasicAuthentication);

                // If you also want to attempt to zero out the string's underlying memory (though with limitations due to string immutability):
                if (basicAuthentication != null)
                {
                    GCHandle handle = GCHandle.Alloc(basicAuthentication, GCHandleType.Pinned);
                    for (int i = 0; i < basicAuthentication.Length; i++)
                    {
                        Marshal.WriteByte(handle.AddrOfPinnedObject() + i * 2, 0);
                    }
                    handle.Free();
                }
            }
        }

        private static string? GetIssueKey(TrackTime trackTime)
        {
            Match match = JiraIssueKeyRegex().Match(trackTime.Title);
            if (!match.Success)
                return null;

            return match.Groups[1].Value;
        }

        private static async Task<StringContent> GetContentAsync(TrackTime trackTime)
        {
            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.Converters.Add(new JiraDateTimeConverter());

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, new Worklog(trackTime), jsonSerializerOptions);
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            return new StringContent(await reader.ReadToEndAsync(), Encoding.UTF8, "application/json");
        }

        internal static async Task GetNewJiraTokenAsync()
        {
            string jiraEmail = FancyPotato.Dialog.InputBox.ShowDialog("Jira E-Mail");
            byte[] base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{jiraEmail}:{FancyPotato.Dialog.InputBox.ShowDialog("Jira Key")}")));
            byte[] jiraToken = ProtectedData.Protect(base64, null, DataProtectionScope.CurrentUser);
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
            SettingsModel.Instance.NotifyPropertyChanged(nameof(SettingsModel.Instance.JiraIsEnabled));
        }
    }
}
