namespace DailyExpenseTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Navigated += OnShellNavigated;
        KeyboardWatcher.ShellHandler = OnKeyboard;
        _ = Data.Store.InitializeAsync();
        _ = Task.Run(async () =>
        {
            await Task.Delay(4000);
            await UpdateChecker.OnAppStartAsync();
        });
    }

    void OnKeyboard()
    {
        try { MainTabPage.Current?.GetType(); } catch { }
    }

    void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        try
        {
            var loc = e.Current?.Location?.OriginalString ?? "";
            var q = loc.IndexOf('?');
            if (q >= 0) loc = loc.Substring(0, q);
            if (loc.Trim('/').Equals("main", StringComparison.OrdinalIgnoreCase)) return;
        }
        catch { }
    }
}
