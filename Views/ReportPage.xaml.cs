using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public partial class ReportPage : ContentPage
{
    DateTime _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    bool _appeared;
    bool _animateFirstLoad;

    public ReportPage()
    {
        InitializeComponent();
        Store.Changed -= OnStoreChanged;
        RangeBox.MonthStep += async d =>
        {
            _month = _month.AddMonths(d);
            await LoadAsync();
        };
        RootChrome.Attach(this);
    }

    void OnStoreChanged() { if (_appeared) _ = LoadAsync(); }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appeared = true;
        _animateFirstLoad = true;
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;

        // Open range form
        if (Ui.PendingOpenRange)
        {
            Ui.PendingOpenRange = false;
            try { RangeBox.OpenEditor(); } catch { }
        }
        await LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appeared = false;
        Store.Changed -= OnStoreChanged;
        Ui.ReturnRoute = null;   // Reset return route
    }

    async Task LoadAsync()
    {
        try
        {
            var today = DateTime.Today;
            var cur = new DateTime(today.Year, today.Month, 1);
            if (_month > cur) _month = cur;

            bool custom = Period.IsCustom;
            RangeBox.Refresh(_month, _month < cur);

            Period.Resolve(_month, out var start, out var endEx);
            var list = await Store.GetRangeAsync(start, endEx);
            var total = list.Sum(x => x.Amount);
            int days = Period.ElapsedDays(start, endEx);

            TotalLabel.Text = Fmt.Money0(total);
            DaysInfo.Text = days > 0 ? days.ToString(Fmt.Inv) + L.T(" দিনের হিসাব") : L.T("এখনো শুরু হয়নি");

            ShowCompare(Period.Ledger(start, endEx, list), days);

            // Period limit bar
            var limit = Period.PeriodLimit(start, endEx);
            if (limit > 0)
            {
                LimitBox.IsVisible = true;
                var left = limit - total;
                var what = custom ? L.T("রেঞ্জের লিমিট ") : L.T("মাসের লিমিট ");
                LimitLabel.Text = left >= 0
                    ? what + Fmt.Money0(limit) + L.T(" · বাকি ") + Fmt.Money0(left)
                    : what + Fmt.Money0(limit) + " · " + Fmt.Money0(-left) + L.T(" বেশি");
                LimitLabel.TextColor = left >= 0 ? Ui.Ink : Ui.Red;
                double frac = (double)(total / limit);
                LimitBarHost.Content = Ui.Bar(frac, Ui.LimitColor(frac), null, 12);
            }
            else
            {
                LimitBox.IsVisible = false;
            }

            BuildHistory(start, endEx, list, total);

            // Percent by category
            ItemsStack.Children.Clear();
            if (list.Count == 0 || total <= 0)
            {
                ItemsStack.Add(new Label { Text = L.T("এই সময়ে কোনো খরচ নেই"), TextColor = Ui.Muted });
                return;
            }

            var groups = list
                .GroupBy(x => x.Cat)
                .Select(g => new { Name = g.Key, Sum = g.Sum(x => x.Amount), Count = g.Count(), Items = g.ToList() })
                .OrderByDescending(x => x.Sum)
                .ToList();

            foreach (var grp in groups)
            {
                double share = (double)(grp.Sum / total);
                var color = Ui.ItemColor(grp.Name);

                var nameBox = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
                nameBox.Add(new Label { Text = grp.Name, FontSize = 16, FontAttributes = FontAttributes.Bold });
                nameBox.Add(new Label { Text = grp.Count.ToString(Fmt.Inv) + L.T("টি এন্ট্রি"), FontSize = 12, TextColor = Ui.Muted });

                var numBox = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
                numBox.Add(new Label
                {
                    Text = (share * 100).ToString("0.#", Fmt.Inv) + "%",
                    FontSize = 17,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = color,
                    HorizontalTextAlignment = TextAlignment.End
                });
                numBox.Add(new Label { Text = Fmt.Money0(grp.Sum), FontSize = 12, TextColor = Ui.Muted, HorizontalTextAlignment = TextAlignment.End });

                var top = new Grid
                {
                    ColumnSpacing = 10,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    }
                };
                top.Add(Ui.ItemBadge(grp.Name, 36), 0);
                top.Add(nameBox, 1);
                top.Add(numBox, 2);

                var box = new VerticalStackLayout { Spacing = 6 };
                box.Add(top);
                box.Add(Ui.Bar(share, color, null, 8));

                // Category breakdown; tap
                var parts = InsideParts(grp.Items);
                if (parts.Count > 0)
                {
                    var insideBox = new VerticalStackLayout { Spacing = 4, IsVisible = false, Margin = new Thickness(46, 2, 0, 0) };
                    foreach (var p in parts)
                    {
                        var row = new Grid
                        {
                            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
                        };
                        row.Add(new Label { Text = "• " + p.Key, FontSize = 14, LineBreakMode = LineBreakMode.TailTruncation }, 0);
                        row.Add(new Label { Text = Fmt.Money0(p.Sum), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = color }, 1);
                        insideBox.Add(row);
                    }

                    var hint = new Label { Text = L.T("▾ ভিতরে কী কী আছে দেখুন"), FontSize = 12, TextColor = color, FontAttributes = FontAttributes.Bold, Margin = new Thickness(46, 0, 0, 0) };
                    box.Add(hint);
                    box.Add(insideBox);
                    box.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            insideBox.IsVisible = !insideBox.IsVisible;
                            hint.Text = insideBox.IsVisible ? L.T("▴ লুকান") : L.T("▾ ভিতরে কী কী আছে দেখুন");
                        })
                    });
                }

                ItemsStack.Add(box);
            }

            if (_animateFirstLoad)
            {
                _animateFirstLoad = false;
                _ = AnimateReportAsync();
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Report.Load", ex);
        }
    }

    async Task AnimateReportAsync()
    {
        try
        {
            await Task.WhenAll(
                Ui.AnimateInAsync(ItemsStack, 220, 0),
                Ui.AnimateInAsync(ListStack, 220, 70));
        }
        catch (Exception ex) { AppLog.Error("Report.Animation", ex); }
    }

    // Sub-split of a
    static List<(string Key, decimal Sum)> InsideParts(List<Expense> items)
    {
        var parts = items
            .GroupBy(x =>
            {
                if (!string.Equals(x.Item, x.Cat, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Item))
                    return x.Item;
                var n = (x.Note ?? "").Split('·')[0].Trim();
                return n.Length == 0 ? L.T("নোট ছাড়া") : n;
            })
            .Select(g => (Key: g.Key, Sum: g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Sum)
            .ToList();

        if (parts.Count == 1 && parts[0].Key == L.T("নোট ছাড়া")) return new();
        return parts;
    }

    // Total spent vs
    void ShowCompare(List<DayBal> ledger, int days)
    {
        if (days <= 0)
        {
            CmpTitle.Text = L.T("লিমিটের তুলনায়");
            CmpLabel.Text = "—";
            CmpLabel.TextColor = Ui.Muted;
            CmpInfo.Text = L.T("এই সময় এখনো শুরু হয়নি");
            return;
        }

        var allowed = ledger.Sum(x => x.Allowed);
        if (allowed <= 0)
        {
            CmpTitle.Text = L.T("লিমিটের তুলনায়");
            CmpLabel.Text = "—";
            CmpLabel.TextColor = Ui.Muted;
            CmpInfo.Text = Period.IsCustom ? L.T("রেঞ্জ বদলান চেপে লিমিট দিন") : L.T("সেটিংসে লিমিট দিন");
            return;
        }

        var diff = allowed - ledger.Sum(x => x.Spent);
        if (diff >= 0)
        {
            CmpTitle.Text = L.T("লিমিটের চেয়ে কম খরচ");
            CmpLabel.TextColor = Ui.Green;
        }
        else
        {
            CmpTitle.Text = L.T("লিমিটের চেয়ে বেশি খরচ");
            CmpLabel.TextColor = Ui.Red;
        }
        CmpLabel.Text = Fmt.Money0(diff < 0 ? -diff : diff);
        CmpInfo.Text = days.ToString(Fmt.Inv) + L.T(" দিনে অনুমোদিত ") + Fmt.Money0(allowed);
    }

    // history (per day)

    void BuildHistory(DateTime start, DateTime endEx, List<Expense> list, decimal total)
    {
        var today = DateTime.Today;
        var last = endEx.AddDays(-1) > today ? today : endEx.AddDays(-1);
        int dayCount = (last - start).Days + 1;

        HistTotal.Text = L.T("মোট খরচ ") + Fmt.Money0(total) + (dayCount > 0 ? " · " + dayCount.ToString(Fmt.Inv) + L.T(" দিন") : "");

        var ledger = Period.Ledger(start, endEx, list);
        if (ledger.Count > 0 && ledger.Any(x => x.Allowed > 0))
        {
            var diff = ledger.Sum(x => x.Diff);
            HistCmp.Text = diff >= 0
                ? L.T("লিমিট থেকে ") + Fmt.Money0(diff) + L.T(" কম খরচ হয়েছে")
                : L.T("লিমিটের চেয়ে ") + Fmt.Money0(-diff) + L.T(" বেশি খরচ হয়েছে");
            HistCmp.TextColor = diff >= 0 ? Ui.Green : Ui.Red;
            HistCmp.IsVisible = true;
        }
        else
        {
            HistCmp.Text = "";
            HistCmp.IsVisible = false;
        }

        ListStack.Children.Clear();
        if (dayCount <= 0)
        {
            ListStack.Add(new Label { Text = L.T("এই সময় এখনো শুরু হয়নি"), TextColor = Ui.Muted, Margin = new Thickness(0, 20) });
            return;
        }

        var byDay = list.GroupBy(x => x.Date.Date).ToDictionary(g => g.Key, g => g.ToList());
        bool includeEmpty = dayCount <= 62;   // Skip empty days on long ranges

        for (var d = last; d >= start; d = d.AddDays(-1))
        {
            byDay.TryGetValue(d, out var entries);
            if (entries == null && !includeEmpty) continue;
            ListStack.Add(DayRow(d, entries ?? new List<Expense>(), Period.DailyLimitFor(d), today));
        }

        if (ListStack.Children.Count == 0)
            ListStack.Add(new Label { Text = L.T("এই সময়ে কোনো খরচ নেই"), TextColor = Ui.Muted, Margin = new Thickness(0, 20) });
    }

    // Day card; tap
    View DayRow(DateTime day, List<Expense> entries, decimal dailyLimit, DateTime today)
    {
        var sum = entries.Sum(x => x.Amount);
        var hasSpend = sum > 0;

        string status = "";
        Color statusColor = Ui.Muted;
        Color badgeBg = Ui.Gray;
        Color badgeFg = Ui.Muted;
        if (dailyLimit > 0)
        {
            var diff = dailyLimit - sum;
            if (diff >= 0)
            {
                status = day == today ? L.T("বাকি ") + Fmt.Money0(diff) : Fmt.Money0(diff) + L.T(" কম");
                statusColor = Ui.Green;
                if (hasSpend || day != today) { badgeBg = Ui.GreenSoft; badgeFg = Ui.Green; }
            }
            else
            {
                status = Fmt.Money0(-diff) + L.T(" বেশি");
                statusColor = Ui.Red;
                badgeBg = Ui.RedSoft;
                badgeFg = Ui.Red;
            }
        }
        else if (hasSpend)
        {
            badgeBg = Ui.PrimarySoft;
            badgeFg = Ui.Primary;
        }

        // left: date badge
        var badgeBox = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        badgeBox.Add(new Label { Text = day.Day.ToString(Fmt.Inv), FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = badgeFg, HorizontalTextAlignment = TextAlignment.Center });
        badgeBox.Add(new Label { Text = Fmt.BnWeekdayShort(day), FontSize = 11, TextColor = badgeFg, HorizontalTextAlignment = TextAlignment.Center });
        var badge = new Border
        {
            WidthRequest = 50,
            HeightRequest = 56,
            StrokeThickness = 0,
            BackgroundColor = badgeBg,
            VerticalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = badgeBox
        };

        // middle: weekday +
        var mid = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        mid.Add(new Label
        {
            Text = Fmt.DayTitle(day),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = hasSpend ? Ui.Ink : Ui.Muted,
            LineBreakMode = LineBreakMode.TailTruncation
        });
        string what;
        if (entries.Count == 0) what = L.T("কোনো খরচ নেই");
        else
        {
            var names = entries.Select(x => string.IsNullOrWhiteSpace(x.Item) ? "Extra" : x.Item).Distinct().ToList();
            what = entries.Count.ToString(Fmt.Inv) + L.T("টি এন্ট্রি · ") + string.Join(", ", names);
        }
        mid.Add(new Label { Text = what, FontSize = 12, TextColor = Ui.Muted, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation });

        // right: amount +
        var right = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        right.Add(new Label
        {
            Text = Fmt.Money0(sum),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = hasSpend ? Ui.Primary : Ui.Muted,
            HorizontalTextAlignment = TextAlignment.End
        });
        if (status.Length > 0)
            right.Add(new Label
            {
                Text = status,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = statusColor,
                HorizontalTextAlignment = TextAlignment.End
            });

        var g = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        g.Add(badge, 0);
        g.Add(mid, 1);
        g.Add(right, 2);

        var b = new Border
        {
            Content = g,
            Padding = new Thickness(12, 10),
            BackgroundColor = Ui.Surface,
            Stroke = Ui.Line,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Ui.OpenDay(this, day)) });
        return b;
    }
}
