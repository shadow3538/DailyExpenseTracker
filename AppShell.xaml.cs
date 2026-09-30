namespace DailyExpenseTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Navigated += OnShellNavigated;
        KeyboardWatcher.ShellHandler = OnKeyboard;

        // Update check
        _ = Task.Run(async () =>
        {
            await Task.Delay(4000);
            await UpdateChecker.OnAppStartAsync();
        });
    }

    void OnKeyboard()
    {
        try
        {
            if (CurrentPage != null) SetTabBarIsVisible(CurrentPage, !KeyboardWatcher.Visible);
        }
        catch { }
    }

    void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        try
        {
            var loc = e.Current?.Location?.OriginalString ?? "";
            var q = loc.IndexOf('?');
            if (q >= 0) loc = loc.Substring(0, q);
            var route = loc.Trim('/');
            if (route.Length == 0) return;
            var slash = route.LastIndexOf('/');
            if (slash >= 0) route = route.Substring(slash + 1);

            if (route != Ui.CurrentRoute)
            {
                Ui.PrevRoute = Ui.CurrentRoute;
                Ui.CurrentRoute = route;
            }
        }
        catch { }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {

            if (RootChrome.Active is { IsOpen: true } chrome)
            {
                _ = chrome.CloseAsync();
                return true;
            }
            var route = Ui.CurrentRoute;
            if (!string.IsNullOrEmpty(route) && route != "home")
            {
                _ = Ui.GoTo("home");
                return true;
            }
        }
        catch { }
        return base.OnBackButtonPressed();
    }
}
