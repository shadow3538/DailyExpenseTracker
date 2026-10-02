using System.Globalization;

namespace DailyExpenseTracker;

public static class Profile
{

    public static event Action? Changed;

    public static void ResetListeners() => Changed = null;

    public static void RaiseChanged() => MainThread.BeginInvokeOnMainThread(() => Changed?.Invoke());

    static string G(string key) => Preferences.Default.Get("pf_" + key, "");
    static void S(string key, string? value) => Preferences.Default.Set("pf_" + key, (value ?? "").Trim());

    public static string Name { get => G("name"); set => S("name", value); }
    public static string Phone { get => G("phone"); set => S("phone", value); }
    public static string Email { get => G("email"); set => S("email", value); }
    public static string Occupation { get => G("job"); set => S("job", value); }
    public static string Address { get => G("address"); set => S("address", value); }
    public static string Bio { get => G("bio"); set => S("bio", value); }
    public static string Gender { get => G("gender"); set => S("gender", value); }
    public static string BloodGroup { get => G("blood"); set => S("blood", value); }

    public static DateTime? Dob
    {
        get => DateTime.TryParseExact(G("dob"), "yyyy-MM-dd", Fmt.Inv, DateTimeStyles.None, out var d) ? d : null;
        set => S("dob", value?.ToString("yyyy-MM-dd", Fmt.Inv));
    }

    public static int? Age
    {
        get
        {
            var d = Dob;
            if (d == null) return null;
            var t = DateTime.Today;
            int a = t.Year - d.Value.Year;
            if (d.Value.Date > t.AddYears(-a)) a--;
            return a < 0 ? null : a;
        }
    }

    public static string? PhotoPath
    {
        get
        {
            var n = G("photo");
            if (n.Length == 0) return null;
            var p = System.IO.Path.Combine(FileSystem.AppDataDirectory, n);
            return File.Exists(p) ? p : null;
        }
    }

    public static bool IsEmpty =>
        Name.Length == 0 && PhotoPath == null && Phone.Length == 0 && Email.Length == 0;

    public static string InitialOf(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        return StringInfo.GetNextTextElement(name.Trim()).ToUpperInvariant();
    }

    public static void CommitPhoto(string? tempPath, bool remove)
    {
        try
        {
            var oldName = G("photo");

            if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
            {
                var name = "profile_" + DateTime.Now.Ticks.ToString(Fmt.Inv) + ".jpg";
                var dest = System.IO.Path.Combine(FileSystem.AppDataDirectory, name);
                File.Copy(tempPath, dest, true);
                try { File.Delete(tempPath); } catch { }
                S("photo", name);
            }
            else if (remove)
            {
                S("photo", "");
            }
            else return;

            if (oldName.Length > 0)
            {
                try { File.Delete(System.IO.Path.Combine(FileSystem.AppDataDirectory, oldName)); } catch { }
            }
        }
        catch { }
    }
}
