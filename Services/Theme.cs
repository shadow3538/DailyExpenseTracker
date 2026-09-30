namespace DailyExpenseTracker;

// Light / dark
public static class Theme
{
    public const string Light = "light";
    public const string Dark = "dark";
    public const string System = "system";

    public static string Mode
    {
        get => Preferences.Default.Get("theme", System);
        private set => Preferences.Default.Set("theme", value);
    }

    // Dark colors active?
    public static bool IsDark
    {
        get
        {
            var m = Mode;
            if (m == Dark) return true;
            if (m == Light) return false;
            try { return Application.Current?.RequestedTheme == AppTheme.Dark; } catch { return false; }
        }
    }

    // Apply on start
    public static void Apply()
    {
        try
        {
            var app = Application.Current;
            if (app == null) return;
            app.UserAppTheme = Mode switch
            {
                Dark => AppTheme.Dark,
                Light => AppTheme.Light,
                _ => AppTheme.Unspecified
            };
            PushResources();
        }
        catch { }
    }

    // Push theme colors
    static void PushResources()
    {
        var r = Application.Current?.Resources;
        if (r == null) return;
        r["Bg"] = Ui.PageBg;
        r["Ink"] = Ui.Ink;
        r["MutedText"] = Ui.Muted;
        r["LineColor"] = Ui.Line;
        r["LineBrush"] = new SolidColorBrush(Ui.Line);
        r["RedText"] = Ui.Red;
        r["Primary"] = Ui.Primary;
        r["SurfaceColor"] = Ui.Surface;
        r["FieldColor"] = Ui.FieldBg;
        r["SepColor"] = Ui.Sep;
        r["BarColor"] = Ui.NavBar;
        r["PlaceholderCol"] = IsDark ? Color.FromArgb("#6F7F8B") : Color.FromArgb("#9AA5B1");
        r["TabUnselected"] = IsDark ? Color.FromArgb("#7C8A96") : Color.FromArgb("#8A94A0");
    }

    // Save mode and
    public static async Task SetAsync(string mode, bool goToSettings = true)
    {
        Mode = mode is Dark or Light ? mode : System;
        Apply();
        await RebuildAsync(goToSettings);
    }

    // Follow phone theme
    public static void HookSystemChange()
    {
        try
        {
            var app = Application.Current;
            if (app == null) return;
            app.RequestedThemeChanged -= OnSystemThemeChanged;
            app.RequestedThemeChanged += OnSystemThemeChanged;
        }
        catch { }
    }

    static AppTheme _lastSeen = AppTheme.Unspecified;

    static void OnSystemThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (Mode != System) return;
        if (e.RequestedTheme == _lastSeen) return;
        _lastSeen = e.RequestedTheme;
        PushResources();
        _ = RebuildAsync(false);
    }

    static Task RebuildAsync(bool goToSettings) => AppRebuild.RunAsync(goToSettings);
}
