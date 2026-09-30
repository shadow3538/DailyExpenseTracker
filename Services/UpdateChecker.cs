using System.Text.RegularExpressions;
using System.Text.Json;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace DailyExpenseTracker;

public sealed record UpdateResult(bool Ok, bool HasUpdate, string Latest, string Url);

public static class UpdateChecker
{
    public const string ChannelId = "hisab_updates";
    public const string ActUpdate = "hisab.update.check";
    const int AlarmCode = 9101;
    const int NotifyId = 9102;

    // Daily alarm
    const int CheckMinutes = 12 * 60;

    // Repository URL
    static string Repo => AppInfo.GitHubUrl.TrimEnd('/');

    // User setting
    public static bool On
    {
        get => Preferences.Default.Get("upd_on", true);
        set => Preferences.Default.Set("upd_on", value);
    }

    public static string LatestSeen => Preferences.Default.Get("upd_latest", "");

    public static string LatestUrl => Preferences.Default.Get("upd_url", "");

    static string LastNotified
    {
        get => Preferences.Default.Get("upd_notified", "");
        set => Preferences.Default.Set("upd_notified", value);
    }

    static DateTime LastCheck
    {
        get => new(Preferences.Default.Get("upd_last_ticks", 0L), DateTimeKind.Utc);
        set => Preferences.Default.Set("upd_last_ticks", value.ToUniversalTime().Ticks);
    }

    public static bool UpdateKnown => IsNewer(LatestSeen, AppInfo.Version);

    static bool TryParse(string? s, out Version v)
    {
        v = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(s)) return false;
        var m = Regex.Match(s.Trim(), @"\d+(\.\d+){0,3}");
        if (!m.Success) return false;
        var txt = m.Value;
        if (!txt.Contains('.')) txt += ".0";
        bool ok = Version.TryParse(txt, out var parsed);
        v = parsed ?? new Version(0, 0);
        return ok;
    }

    public static bool IsNewer(string? remote, string? local) =>
        TryParse(remote, out var r) && TryParse(local, out var l) && r > l;

    // GitHub client
    static HttpClient NewClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("PocketNama-UpdateCheck");
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    public static async Task<UpdateResult> CheckAsync(bool notify)
    {
        try
        {
            if (!InfoLinks.Has(AppInfo.GitHubUrl)) return new UpdateResult(false, false, "", "");

            var parts = new Uri(Repo).AbsolutePath.Trim('/').Split('/');
            if (parts.Length < 2) return new UpdateResult(false, false, "", "");
            var owner = parts[0];
            var repo = parts[1];

            // Version sources
            string latest = "", url = "";
            using var http = NewClient();

            using (var resp = await http.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest"))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("tag_name", out var tag)) latest = tag.GetString() ?? "";
                    if (root.TryGetProperty("html_url", out var html)) url = html.GetString() ?? "";
                }
            }

            if (!TryParse(latest, out _))
            {
                latest = "";
                using var resp = await http.GetAsync($"https://raw.githubusercontent.com/{owner}/{repo}/HEAD/DailyExpenseTracker.csproj");
                if (resp.IsSuccessStatusCode)
                {
                    var txt = await resp.Content.ReadAsStringAsync();
                    var m = Regex.Match(txt, @"<ApplicationDisplayVersion>\s*([^<\s]+)\s*<");
                    if (m.Success) latest = m.Groups[1].Value;
                    url = "";
                }
            }

            if (!TryParse(latest, out _)) return new UpdateResult(false, false, "", "");
            if (!InfoLinks.Has(url)) url = InfoLinks.Has(AppInfo.DownloadUrl) ? AppInfo.DownloadUrl : Repo;

            LastCheck = DateTime.UtcNow;
            Preferences.Default.Set("upd_latest", latest.TrimStart('v', 'V'));
            Preferences.Default.Set("upd_url", url);

            var cleanLatest = latest.TrimStart('v', 'V');
            bool has = IsNewer(cleanLatest, AppInfo.Version);
            if (has && notify && LastNotified != cleanLatest)
            {
                if (Notify(cleanLatest, url)) LastNotified = cleanLatest;
            }
            return new UpdateResult(true, has, cleanLatest, url);
        }
        catch (Exception ex)
        {
            AppLog.Error("Update.Check", ex);
            return new UpdateResult(false, false, "", "");
        }
    }

    // Notification channel
    static void EnsureChannel(Context ctx)
    {
        try
        {
            if ((int)Build.VERSION.SdkInt < 26) return;
            var nm = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            if (nm == null || nm.GetNotificationChannel(ChannelId) != null) return;
            nm.CreateNotificationChannel(new NotificationChannel(ChannelId, L.T("অ্যাপ আপডেট", "App updates"), NotificationImportance.Default));
        }
        catch { }
    }

    // Update notice
    static bool Notify(string version, string url)
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            if (!Reminders.NotificationsAllowed()) return false;
            EnsureChannel(ctx);

            var view = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
            view.AddFlags(ActivityFlags.NewTask);
            var pi = PendingIntent.GetActivity(ctx, NotifyId, view, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var v = version.TrimStart('v', 'V');
            var title = L.T("পকেটনামা — নতুন আপডেট", "PocketNama — update available");
            var text = L.T(
                $"নতুন ভার্সন {v} এসেছে (আপনার ভার্সন {AppInfo.Version})। ডাউনলোড করতে ট্যাপ করুন।",
                $"Version {v} is available (you have {AppInfo.Version}). Tap to download.");

            var b = new NotificationCompat.Builder(ctx, ChannelId)
                .SetSmallIcon(Resource.Drawable.ic_notify)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
                .SetAutoCancel(true)
                .SetContentIntent(pi);

            var nm = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            nm?.Notify(NotifyId, b.Build());
            return true;
        }
        catch (Exception ex)
        {
            AppLog.Error("Update.Notify", ex);
            return false;
        }
    }

    // Alarm intent
    static PendingIntent Pi(Context ctx)
    {
        var i = new Intent(ctx, typeof(ReminderReceiver));
        i.SetAction(ActUpdate);
        return PendingIntent.GetBroadcast(ctx, AlarmCode, i, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    public static void Schedule(Context ctx)
    {
        try
        {
            var am = (AlarmManager?)ctx.GetSystemService(Context.AlarmService);
            if (am == null) return;
            if (!On || !InfoLinks.Has(AppInfo.GitHubUrl)) { Cancel(ctx); return; }

            var t = DateTime.Today.AddMinutes(CheckMinutes);
            if (t <= DateTime.Now.AddMinutes(1)) t = t.AddDays(1);
            long ms = new DateTimeOffset(t).ToUnixTimeMilliseconds();

            if ((int)Build.VERSION.SdkInt >= 23) am.SetAndAllowWhileIdle(AlarmType.RtcWakeup, ms, Pi(ctx));
            else am.Set(AlarmType.RtcWakeup, ms, Pi(ctx));
        }
        catch (Exception ex) { AppLog.Error("Update.Schedule", ex); }
    }

    public static void Cancel(Context ctx)
    {
        try { ((AlarmManager?)ctx.GetSystemService(Context.AlarmService))?.Cancel(Pi(ctx)); } catch { }
    }

    public static async Task OnAppStartAsync()
    {
        try
        {
            if (!On || !InfoLinks.Has(AppInfo.GitHubUrl)) return;
            var ctx = global::Android.App.Application.Context;
            Schedule(ctx);

            if (!Preferences.Default.Get("upd_perm_asked", false))
            {
                Preferences.Default.Set("upd_perm_asked", true);
                MainThread.BeginInvokeOnMainThread(Reminders.AskPermission);
            }

            // Startup fallback
            if (DateTime.UtcNow - LastCheck > TimeSpan.FromHours(20))
                await CheckAsync(notify: true);
        }
        catch (Exception ex) { AppLog.Error("Update.Start", ex); }
    }
}
