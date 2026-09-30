using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

public class WidgetDay
{
    public DateTime Day { get; set; }
    public decimal Spent { get; set; }
    public decimal Allowed { get; set; }
}

public class WidgetData
{
    public DateTime Today { get; set; }
    public decimal TodaySpent { get; set; }
    public decimal TodayLimit { get; set; }
    public decimal PeriodSpent { get; set; }
    public decimal PeriodLimit { get; set; }
    public bool Custom { get; set; }

    public List<WidgetDay> Week { get; set; } = new();

    public static async Task<WidgetData> LoadAsync()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        Period.Resolve(monthStart, out var pStart, out var pEnd);

        var from = pStart < today.AddDays(-6) ? pStart : today.AddDays(-6);
        var to = pEnd > today.AddDays(1) ? pEnd : today.AddDays(1);
        var all = await Store.GetRangeAsync(from, to);

        var d = new WidgetData
        {
            Today = today,
            Custom = Period.IsCustom,
            TodaySpent = all.Where(x => x.Date.Date == today).Sum(x => x.Amount),
            TodayLimit = Period.DailyLimitFor(today),
            PeriodSpent = all.Where(x => x.Date >= pStart && x.Date < pEnd).Sum(x => x.Amount),
            PeriodLimit = Period.PeriodLimit(pStart, pEnd)
        };

        for (int i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            d.Week.Add(new WidgetDay
            {
                Day = day,
                Spent = all.Where(x => x.Date.Date == day).Sum(x => x.Amount),
                Allowed = Period.DailyLimitFor(day)
            });
        }
        return d;
    }
}
