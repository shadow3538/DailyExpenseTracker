using System.Diagnostics;

namespace DailyExpenseTracker;

// Diagnostic log; no
public static class AppLog
{
    public static void Error(string area, Exception ex)
    {
        Debug.WriteLine($"[DailyExpenseTracker:{area}] {ex.GetType().Name}: {ex.Message}");
    }

    public static void Info(string area, string message)
    {
        Debug.WriteLine($"[DailyExpenseTracker:{area}] {message}");
    }
}
