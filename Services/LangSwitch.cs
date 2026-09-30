namespace DailyExpenseTracker;

// Switch language: names
public static class LangSwitch
{
    static bool _busy;

    public static async Task ApplyAsync(string lang, bool goToSettings)
    {
        if (_busy) return;          // Ignore double tap
        _busy = true;
        try
        {
            Ui.Toast(L.T("অপেক্ষা করুন...", "Please wait..."));
            await Task.Yield();

            // Quiet mode: no
            Data.Store.Quiet = true;
            try
            {
                L.Set(lang);
                await Data.Store.LocalizeDefaultsAsync();
                await Reminders.RescheduleAsync();
            }
            finally { Data.Store.Quiet = false; }

            try { ExpenseWidget.Refresh(global::Android.App.Application.Context); } catch { }

            await AppRebuild.RunAsync(goToSettings);
        }
        catch { }
        finally { _busy = false; }
    }
}

// Rebuild AppShell (language
public static class AppRebuild
{
    static bool _running;

    public static async Task RunAsync(bool goToSettings)
    {
        if (_running) return;
        _running = true;
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Let button click
                await Task.Delay(80);

                // Old pages stop
                Data.Store.ResetListeners();
                Profile.ResetListeners();

                var w = Application.Current?.Windows.FirstOrDefault();
                if (w != null)
                {
                    Ui.CurrentRoute = "home";
                    Ui.PrevRoute = null;
                    w.Page = new AppShell();
                }
            });

            if (goToSettings)
            {
                await Task.Delay(350);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try { await Ui.GoTo("settings"); } catch { }
                });
            }
        }
        catch { }
        finally { _running = false; }
    }
}
