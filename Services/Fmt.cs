using System.Globalization;

namespace DailyExpenseTracker;

public static class Fmt
{
    public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    static readonly string[] BnMonths =
    {
        "জানুয়ারি", "ফেব্রুয়ারি", "মার্চ", "এপ্রিল", "মে", "জুন",
        "জুলাই", "আগস্ট", "সেপ্টেম্বর", "অক্টোবর", "নভেম্বর", "ডিসেম্বর"
    };

    static readonly string[] BnDays = { "রবিবার", "সোমবার", "মঙ্গলবার", "বুধবার", "বৃহস্পতিবার", "শুক্রবার", "শনিবার" };
    static readonly string[] BnDaysShort = { "রবি", "সোম", "মঙ্গল", "বুধ", "বৃহ", "শুক্র", "শনি" };
    static readonly string[] EnMonths = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    static readonly string[] EnDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
    static readonly string[] EnDaysShort = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

    static string[] Months => L.IsEn ? EnMonths : BnMonths;
    static string[] Days => L.IsEn ? EnDays : BnDays;
    static string[] DaysShort => L.IsEn ? EnDaysShort : BnDaysShort;

    public static string DayHint(DateTime d)
    {
        if (d.Date == DateTime.Today) return L.T("আজ", "Today");
        if (d.Date == DateTime.Today.AddDays(-1)) return L.T("গতকাল", "Yesterday");
        return Days[(int)d.DayOfWeek];
    }

    public static string BnDayShort(DateTime d) => d.Date == DateTime.Today ? L.T("আজ", "Today") : DaysShort[(int)d.DayOfWeek];

    public static string Short(decimal v) =>
        v >= 1000 ? (v / 1000).ToString("0.#", Inv) + "k" : v.ToString("0", Inv);

    public static string Money(decimal v) => AppSettings.Currency + v.ToString("N2", Inv);

    public static string Money0(decimal v) =>
        AppSettings.Currency + (v == Math.Round(v) ? v.ToString("N0", Inv) : v.ToString("N2", Inv));

    public static string DayMonth(DateTime d) => d.Day.ToString(Inv) + " " + Months[d.Month - 1];

    public static string BnWeekday(DateTime d) => Days[(int)d.DayOfWeek];

    public static string BnWeekdayShort(DateTime d) => DaysShort[(int)d.DayOfWeek];

    public static string MonthTitle(DateTime d) => Months[d.Month - 1] + " " + d.Year.ToString(Inv);

    public static string RangeTitle(DateTime a, DateTime b)
    {
        var left = a.Year == b.Year ? a.ToString("dd MMM", Inv) : a.ToString("dd MMM yyyy", Inv);
        return left + " – " + b.ToString("dd MMM yyyy", Inv);
    }

    public static string DayTitle(DateTime d)
    {
        var s = d.ToString("dd MMM yyyy, ddd", Inv);
        if (d.Date == DateTime.Today) return L.T("আজ", "Today") + " · " + s;
        if (d.Date == DateTime.Today.AddDays(-1)) return L.T("গতকাল", "Yesterday") + " · " + s;
        return s;
    }

    public static bool TryAmount(string? s, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        var sb = new System.Text.StringBuilder();
        foreach (var ch in s.Trim())
        {
            if (ch >= '০' && ch <= '৯') sb.Append((char)('0' + (ch - '০')));
            else if (ch == ',' || ch == ' ' || ch == '\u00A0') continue;
            else if (ch == '।') sb.Append('.');
            else sb.Append(ch);
        }
        if (!decimal.TryParse(sb.ToString(), NumberStyles.Number, Inv, out value)) return false;
        return value >= 0;
    }
}
