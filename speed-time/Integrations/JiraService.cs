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
            await UpdateWorklogs(trackTimes.Where(t => t.ContainsAttribute(WorklogAttribute)).ToList());
            await CreateWorklogsAsync(trackTimes.Where(t => !t.ContainsAttribute(WorklogAttribute)).ToList());
        }

        private static async Task CreateWorklogsAsync(List<TrackTime> trackTimes)
        {
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
                    string? issueKey = GetIssueKey(trackTime);
                    if (issueKey is null)
                        continue;

                    trackTime.SetAttribute(IssueKeyAttribute, issueKey);

                    HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"/rest/api/3/issue/{issueKey}/worklog")
                    {
                        Content = await GetContentAsync(trackTime)
                    };

                    HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    using JsonDocument? response = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStreamAsync());

                    if (response is null)
                        continue;

                    // TODO: Show error
                    if (!httpResponseMessage.IsSuccessStatusCode)
                        continue;

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

        private static async Task UpdateWorklogs(List<TrackTime> trackTimes)
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
                    // TODO: Check if changed? Or should the check before that?
                    string? issueKey = trackTime.GetAttribute(IssueKeyAttribute);
                    if (issueKey is null)
                        continue;

                    string? worklogId = trackTime.GetAttribute(WorklogAttribute);
                    if (worklogId is null)
                        continue;

                    HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, $"/rest/api/3/issue/{issueKey}/worklog/{worklogId}")
                    {
                        Content = await GetContentAsync(trackTime)
                    };

                    HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    string response = await httpResponseMessage.Content.ReadAsStringAsync();
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

        internal static async Task DeleteWorklogAsync(TrackTime trackTime)
        {
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

                string? issueKey = GetIssueKey(trackTime);
                if (issueKey is null)
                    return;

                string? worklogId = trackTime.GetAttribute(WorklogAttribute);
                if (worklogId is null)
                    return;

                HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"/rest/api/3/issue/{issueKey}/worklog/{worklogId}");
                HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
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
        }
    }
}
