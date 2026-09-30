namespace DailyExpenseTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Navigated += OnShellNavigated;
        KeyboardWatcher.ShellHandler = OnKeyboard;
    }

    // Hide tab bar
    void OnKeyboard()
    {
        try
        {
            if (CurrentPage != null) SetTabBarIsVisible(CurrentPage, !KeyboardWatcher.Visible);
        }
        catch { }
    }

    // Track tabs
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

    // Handle back
    protected override bool OnBackButtonPressed()
    {
        try
        {
            // close drawer first
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
