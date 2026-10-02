using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public class DayBal
{
    public DateTime Day { get; set; }
    public decimal Spent { get; set; }
    public decimal Allowed { get; set; }

    public decimal Diff => Allowed - Spent;

    public decimal Cum { get; set; }
}

public static class Period
{
    public static bool IsCustom
    {
        get => Preferences.Default.Get("period_custom", false);
        set => Preferences.Default.Set("period_custom", value);
    }

    public static DateTime CustomStart
    {
        get => Preferences.Default.Get("period_start", new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        set => Preferences.Default.Set("period_start", value.Date);
    }

    public static DateTime CustomEnd
    {
        get => Preferences.Default.Get("period_end", DateTime.Today).Date;
        set => Preferences.Default.Set("period_end", value.Date);
    }

    public static decimal RangeLimit
    {
        get => (decimal)Preferences.Default.Get("period_limit", 0.0);
        set => Preferences.Default.Set("period_limit", (double)value);
    }

    static DateTime SafeEnd => CustomEnd < CustomStart ? CustomStart : CustomEnd;

    public static int RangeDays => (SafeEnd - CustomStart).Days + 1;

    public static void Resolve(DateTime monthStart, out DateTime start, out DateTime endExclusive)
    {
        if (IsCustom)
        {
            start = CustomStart;
            endExclusive = SafeEnd.AddDays(1);
        }
        else
        {
            start = monthStart;
            endExclusive = monthStart.AddMonths(1);
        }
    }

    public static int ElapsedDays(DateTime start, DateTime endExclusive)
    {
        var stop = endExclusive < DateTime.Today.AddDays(1) ? endExclusive : DateTime.Today.AddDays(1);
        var n = (stop - start).Days;
        return n < 0 ? 0 : n;
    }

    public static string Title(DateTime monthStart)
    {
        if (!IsCustom) return Fmt.MonthTitle(monthStart);
        return Fmt.RangeTitle(CustomStart, SafeEnd);
    }

    public static decimal DailyLimitFor(DateTime day)
    {
        day = day.Date;
        if (IsCustom && RangeLimit > 0 && day >= CustomStart && day <= SafeEnd)
            return RangeLimit / RangeDays;
        if (AppSettings.DailyLimit > 0) return AppSettings.DailyLimit;
        if (AppSettings.MonthlyLimit > 0)
            return AppSettings.MonthlyLimit / DateTime.DaysInMonth(day.Year, day.Month);
        return 0;
    }

    public static decimal PeriodLimit(DateTime start, DateTime endExclusive)
    {
        if (IsCustom)
        {
            if (RangeLimit > 0) return RangeLimit;
        }
        else if (AppSettings.MonthlyLimit > 0) return AppSettings.MonthlyLimit;

        decimal sum = 0;
        for (var d = start; d < endExclusive; d = d.AddDays(1)) sum += DailyLimitFor(d);
        return sum;
    }

    public static List<DayBal> Ledger(DateTime start, DateTime endExclusive, IEnumerable<Expense> list)
    {
        var res = new List<DayBal>();
        var last = endExclusive.AddDays(-1) > DateTime.Today ? DateTime.Today : endExclusive.AddDays(-1);
        if (last < start) return res;

        var byDay = list.GroupBy(x => x.Date.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
        decimal cum = 0;
        for (var d = start; d <= last; d = d.AddDays(1))
        {
            byDay.TryGetValue(d, out var spent);
            var allowed = DailyLimitFor(d);
            cum += allowed - spent;
            res.Add(new DayBal { Day = d, Spent = spent, Allowed = allowed, Cum = cum });
        }
        return res;
    }
}
