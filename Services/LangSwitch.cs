namespace DailyExpenseTracker;

public static class LangSwitch
{
    static bool _busy;

    public static async Task ApplyAsync(string lang, bool goToSettings)
    {
        if (_busy) return;
        _busy = true;
        try
        {
            Data.Store.Quiet = true;
            try
            {
                L.Set(lang);
                await Data.Store.LocalizeDefaultsAsync();
                await Reminders.RescheduleAsync();
            }
            finally { Data.Store.Quiet = false; }

            try { ExpenseWidget.Refresh(global::Android.App.Application.Context); } catch { }

            if (MainTabPage.Current != null)
                await MainTabPage.Current.RefreshAfterPreferenceChangeAsync();

            if (goToSettings)
                await Ui.GoTo("settings");
        }
        catch (Exception ex) { AppLog.Error("Lang.Apply", ex); }
        finally { _busy = false; }
    }
}

public static class AppRebuild
{
    public static async Task RunAsync(bool goToSettings)
    {
        if (MainTabPage.Current != null)
        {
            await MainTabPage.Current.RefreshAfterPreferenceChangeAsync();
            if (goToSettings) await Ui.GoTo("settings");
            return;
        }

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var w = Application.Current?.Windows.FirstOrDefault();
                if (w != null) w.Page = new AppShell();
            });
        }
        catch (Exception ex) { AppLog.Error("App.Rebuild", ex); }
    }
}
