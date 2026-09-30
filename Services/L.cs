namespace DailyExpenseTracker;

public static partial class L
{
    static bool? _en;

    public static bool IsEn
    {
        get
        {
            _en ??= Preferences.Default.Get("lang", "bn") == "en";
            return _en.Value;
        }
    }

    public static bool IsChosen => Preferences.Default.ContainsKey("lang");

    public static void Set(string lang)
    {
        Preferences.Default.Set("lang", lang == "en" ? "en" : "bn");
        _en = lang == "en";
    }

    public static string T(string bn)
    {
        if (!IsEn) return bn;
        return En.TryGetValue(bn, out var e) ? e : bn;
    }

    public static string T(string bn, string en) => IsEn ? en : bn;

    public static string ToEn(string bn) => En.TryGetValue(bn, out var e) ? e : bn;
}
