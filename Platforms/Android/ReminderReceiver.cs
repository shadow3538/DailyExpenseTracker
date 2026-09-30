using Android.App;
using Android.Content;

namespace DailyExpenseTracker;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced })]
public class ReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;
        var action = intent.Action ?? "";
        var pending = GoAsync();

        Task.Run(async () =>
        {
            try
            {
                // Restore alarms
                if (action == Intent.ActionBootCompleted || action == Intent.ActionMyPackageReplaced)
                {
                    await Reminders.RescheduleAsync();
                    UpdateChecker.Schedule(context);
                }
                // Check update
                else if (action == UpdateChecker.ActUpdate)
                {
                    await UpdateChecker.CheckAsync(notify: true);
                    UpdateChecker.Schedule(context);
                }
                else if (action == Reminders.ActDaily)
                {
                    Reminders.Show(context, intent.GetStringExtra("title") ?? "", intent.GetStringExtra("text") ?? "",
                        intent.GetStringExtra("route") ?? "add", intent.GetIntExtra("code", 9001));
                    if (Reminders.DailyOn) Reminders.ScheduleDaily(context);
                }
                else if (action.StartsWith(Reminders.ActLoan))
                {
                    Reminders.Show(context, intent.GetStringExtra("title") ?? "", intent.GetStringExtra("text") ?? "",
                        intent.GetStringExtra("route") ?? "settings", intent.GetIntExtra("code", 20000));
                }
            }
            catch { }
            finally
            {
                try { pending?.Finish(); } catch { }
            }
        });
    }
}
