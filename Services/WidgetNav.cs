namespace DailyExpenseTracker;

public static class WidgetNav
{
    public static string? Pending;

    public static async void TryGo()
    {
        try
        {
            var r = Pending;
            if (string.IsNullOrEmpty(r)) return;
            Pending = null;
            if (r == "home") return;

            for (int i = 0; i < 20 && Shell.Current == null; i++) await Task.Delay(100);
            if (Shell.Current == null) return;
            await Task.Delay(150);
            await MainThread.InvokeOnMainThreadAsync(() => Ui.GoTo(r));
        }
        catch { }
    }
}
