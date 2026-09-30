using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

public static class Reminders
{
    public const string ChannelId = "hisab_reminders";
    public const string ActDaily = "hisab.reminder.daily";
    public const string ActLoan = "hisab.reminder.loan.";
    const int DailyCode = 9001;
    const int LoanCodeBase = 20000;

    public static bool DailyOn
    {
        get => Preferences.Default.Get("rem_daily_on", false);
        set => Preferences.Default.Set("rem_daily_on", value);
    }

    public static int DailyMinutes
    {
        get => Preferences.Default.Get("rem_daily_min", 21 * 60);
        set => Preferences.Default.Set("rem_daily_min", value);
    }

    public static bool LoanOn
    {
        get => Preferences.Default.Get("rem_loan_on", false);
        set => Preferences.Default.Set("rem_loan_on", value);
    }

    public static int LoanMinutes
    {
        get => Preferences.Default.Get("rem_loan_min", 9 * 60);
        set => Preferences.Default.Set("rem_loan_min", value);
    }

    public static int LoanDaysBefore
    {
        get => Preferences.Default.Get("rem_loan_days", 0);
        set => Preferences.Default.Set("rem_loan_days", value);
    }

    static string LoanIds
    {
        get => Preferences.Default.Get("rem_loan_ids", "");
        set => Preferences.Default.Set("rem_loan_ids", value);
    }

    public static bool NotificationsAllowed()
    {
        try
        {
            return NotificationManagerCompat.From(global::Android.App.Application.Context).AreNotificationsEnabled();
        }
        catch { return true; }
    }

    public static void AskPermission()
    {
        try
        {
            if ((int)Build.VERSION.SdkInt < 33) return;
            var act = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (act == null) return;
            const string perm = "android.permission.POST_NOTIFICATIONS";
            if (act.CheckSelfPermission(perm) != global::Android.Content.PM.Permission.Granted)
                act.RequestPermissions(new[] { perm }, 4711);
        }
        catch { }
    }

    static void EnsureChannel(Context ctx)
    {
        try
        {
            if ((int)Build.VERSION.SdkInt < 26) return;
            var nm = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            if (nm == null || nm.GetNotificationChannel(ChannelId) != null) return;
            var ch = new NotificationChannel(ChannelId, L.T("রিমাইন্ডার", "Reminders"), NotificationImportance.Default);
            nm.CreateNotificationChannel(ch);
        }
        catch { }
    }

    public static void Show(Context ctx, string title, string text, string route, int id)
    {
        try
        {
            EnsureChannel(ctx);
            var i = new Intent(ctx, typeof(MainActivity));
            i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            i.PutExtra("route", route);
            var pi = PendingIntent.GetActivity(ctx, id, i, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var b = new NotificationCompat.Builder(ctx, ChannelId)
                .SetSmallIcon(Resource.Drawable.ic_notify)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
                .SetAutoCancel(true)
                .SetContentIntent(pi);

            var nm = (NotificationManager?)ctx.GetSystemService(Context.NotificationService);
            nm?.Notify(id, b.Build());
        }
        catch { }
    }

    public static void ShowTest()
    {
        var ctx = global::Android.App.Application.Context;
        Show(ctx, L.T("পকেটনামা", "PocketNama"),
            L.T("নোটিফিকেশন ঠিকমতো কাজ করছে ✔", "Notifications are working ✔"), "home", 9999);
    }

    static PendingIntent Pi(Context ctx, string action, int code, string? title = null, string? text = null, string? route = null)
    {
        var i = new Intent(ctx, typeof(ReminderReceiver));
        i.SetAction(action);
        if (title != null) i.PutExtra("title", title);
        if (text != null) i.PutExtra("text", text);
        if (route != null) i.PutExtra("route", route);
        i.PutExtra("code", code);
        return PendingIntent.GetBroadcast(ctx, code, i, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    static void SetAlarm(AlarmManager am, long ms, PendingIntent pi)
    {
        try
        {
            int sdk = (int)Build.VERSION.SdkInt;
            if (sdk >= 31)
            {
                if (am.CanScheduleExactAlarms()) am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, ms, pi);
                else am.SetAndAllowWhileIdle(AlarmType.RtcWakeup, ms, pi);
            }
            else if (sdk >= 23) am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, ms, pi);
            else am.Set(AlarmType.RtcWakeup, ms, pi);
        }
        catch
        {
            try { am.Set(AlarmType.RtcWakeup, ms, pi); } catch { }
        }
    }

    static long ToMillis(DateTime local) => new DateTimeOffset(local).ToUnixTimeMilliseconds();

    static AlarmManager? Mgr(Context ctx) => (AlarmManager?)ctx.GetSystemService(Context.AlarmService);

    public static void ScheduleDaily(Context ctx)
    {
        var am = Mgr(ctx);
        if (am == null) return;
        var t = DateTime.Today.AddMinutes(DailyMinutes);
        if (t <= DateTime.Now.AddSeconds(5)) t = t.AddDays(1);

        var title = L.T("পকেটনামা", "PocketNama");
        var text = L.T("আজকের খরচগুলো লিখে রেখেছেন তো? এখনই হিসাব যোগ করুন।", "Did you record today's expenses? Add them now.");
        SetAlarm(am, ToMillis(t), Pi(ctx, ActDaily, DailyCode, title, text, "add"));
    }

    static void CancelDaily(Context ctx)
    {
        try { Mgr(ctx)?.Cancel(Pi(ctx, ActDaily, DailyCode)); } catch { }
    }

    public static async Task RescheduleAsync()
    {
        try
        {
            var ctx = global::Android.App.Application.Context;
            var am = Mgr(ctx);
            if (am == null) return;

            foreach (var s in LoanIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(s, out var lid)) continue;
                try { am.Cancel(Pi(ctx, ActLoan + lid, LoanCodeBase + lid)); } catch { }
            }
            LoanIds = "";
            CancelDaily(ctx);
            UpdateChecker.Schedule(ctx);

            if (DailyOn) ScheduleDaily(ctx);

            if (LoanOn)
            {
                var loans = await Store.GetAllLoansAsync();
                var ids = new List<int>();
                foreach (var l in loans)
                {
                    if (l.Settled || !l.HasDue) continue;
                    var t = l.DueDate.Date.AddDays(-LoanDaysBefore).AddMinutes(LoanMinutes);
                    if (t <= DateTime.Now.AddSeconds(5)) continue;

                    var money = Fmt.Money0(l.Remaining);
                    string title, text;
                    if (l.IOwe)
                    {
                        title = L.T("ঋণ শোধের রিমাইন্ডার", "Debt payment reminder");
                        text = L.T(l.Person + " কে " + money + " দেওয়ার তারিখ " + Fmt.DayMonth(l.DueDate),
                                   "Pay " + money + " to " + l.Person + " — due " + Fmt.DayMonth(l.DueDate));
                    }
                    else
                    {
                        title = L.T("পাওনা টাকার রিমাইন্ডার", "Money to receive reminder");
                        text = L.T(l.Person + " এর কাছ থেকে " + money + " পাওয়ার তারিখ " + Fmt.DayMonth(l.DueDate),
                                   "Collect " + money + " from " + l.Person + " — due " + Fmt.DayMonth(l.DueDate));
                    }
                    SetAlarm(am, ToMillis(t), Pi(ctx, ActLoan + l.Id, LoanCodeBase + l.Id, title, text, "settings"));
                    ids.Add(l.Id);
                }
                LoanIds = string.Join(",", ids);
            }
        }
        catch { }
    }
}
