using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DailyExpenseTracker;

// Daily update check
public static class UpdateChecker
{
    const string LastCheckKey = "update_last_check";
    const string NotifiedKey = "update_notified_version";
    static readonly HttpClient Http = CreateClient();

    static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(12)
        };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PocketNama", AppInfo.Version));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    public static async Task CheckDailyAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(AppInfo.GitHubUrl)) return;
            if (!DateTime.TryParse(Preferences.Default.Get(LastCheckKey, ""), out var last)) last = DateTime.MinValue;
            if (last.Date == DateTime.Today) return;

            Preferences.Default.Set(LastCheckKey, DateTime.Now.ToString("O"));

            var api = BuildApiUrl(AppInfo.GitHubUrl);
            if (api.Length == 0) return;

            using var response = await Http.GetAsync(api);
            if (!response.IsSuccessStatusCode) return;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(stream);
            var root = json.RootElement;
            var tag = root.TryGetProperty("tag_name", out var tagValue) ? tagValue.GetString() : null;
            var releaseUrl = root.TryGetProperty("html_url", out var urlValue) ? urlValue.GetString() : null;
            if (!TryGetVersion(tag, out var latest) || !TryGetVersion(AppInfo.Version, out var current)) return;
            if (latest <= current) return;
            if (string.IsNullOrWhiteSpace(releaseUrl)) releaseUrl = AppInfo.GitHubUrl.TrimEnd('/') + "/releases/latest";

            var latestText = tag?.Trim() ?? latest.ToString();
            if (string.Equals(Preferences.Default.Get(NotifiedKey, ""), latestText, StringComparison.OrdinalIgnoreCase)) return;

            if (Reminders.ShowUpdate(
                    L.T("নতুন ভার্সন এসেছে", "New version available"),
                    L.T(
                        $"পকেটনামা {latestText} এসেছে। আপডেট দেখতে ট্যাপ করুন।",
                        $"PocketNama {latestText} is available. Tap to view the update."),
                    releaseUrl!))
            {
                Preferences.Default.Set(NotifiedKey, latestText);
            }
        }
        catch
        {
            // Silent failure
        }
    }

    static string BuildApiUrl(string repoUrl)
    {
        var uri = repoUrl.Trim().TrimEnd('/');
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed)) return "";
        if (!string.Equals(parsed.Host, "github.com", StringComparison.OrdinalIgnoreCase)) return "";

        var parts = parsed.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return "";
        return $"https://api.github.com/repos/{Uri.EscapeDataString(parts[0])}/{Uri.EscapeDataString(parts[1])}/releases/latest";
    }

    static bool TryGetVersion(string? value, out Version version)
    {
        version = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(value)) return false;

        var match = Regex.Match(value, @"\d+(?:\.\d+){0,3}");
        if (!match.Success) return false;
        return Version.TryParse(match.Value, out version);
    }


}
