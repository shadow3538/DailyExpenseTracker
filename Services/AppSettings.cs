namespace DailyExpenseTracker;

public static class AppSettings
{
    public static string Currency => "৳";

    public static decimal DailyLimit
    {
        get => (decimal)Preferences.Default.Get("daily_limit", 0.0);
        set => Preferences.Default.Set("daily_limit", (double)value);
    }

    public static decimal MonthlyLimit
    {
        get => (decimal)Preferences.Default.Get("monthly_limit", 0.0);
        set => Preferences.Default.Set("monthly_limit", (double)value);
    }
}
