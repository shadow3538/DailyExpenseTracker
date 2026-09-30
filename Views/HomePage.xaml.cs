using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public partial class HomePage : ContentPage
{
    const double BarMax = 72;
    const int LedgerRows = 7;

    static readonly Color OnDarkTrack = new Color(1f, 1f, 1f, 0.25f);
    static readonly Color OnDarkWarn = Color.FromArgb("#FFD180");
    static readonly Color OnDarkOver = Color.FromArgb("#FF8A80");
    static readonly Color OnDarkOverText = Color.FromArgb("#FFB4A8");

    bool _appeared;
    bool _animateDashboard;

    public HomePage()
    {
        InitializeComponent();
        Store.Changed -= OnStoreChanged;
        BuildTitleView();
        Profile.Changed += () => { if (_appeared) RefreshTitle(); };
    }

    ContentView _titleAvatar = new();
    Label _titleName = new();

    void BuildTitleView()
    {
        _titleName = new Label
        {
            TextColor = Colors.White,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        var sub = new Label { Text = L.T("পকেটনামা"), TextColor = Color.FromArgb("#CFF3EE"), FontSize = 12 };
        var texts = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        texts.Add(_titleName);
        texts.Add(sub);

        var g = new Grid
        {
            ColumnSpacing = 10,
            Padding = new Thickness(0, 0, 12, 0),
            VerticalOptions = LayoutOptions.Center,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        g.Add(_titleAvatar, 0);
        g.Add(texts, 1);
        g.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Ui.OpenProfile(this)) });
        RefreshTitle();
        RootChrome.Attach(this, g);
    }

    void RefreshTitle()
    {
        try
        {
            _titleAvatar.Content = Ui.Avatar(40, ring: true);
            _titleName.Text = Profile.Name.Length > 0 ? Profile.Name : L.T("প্রোফাইল সেট করুন");
        }
        catch (Exception ex)
        {
            AppLog.Error("Home.RefreshTitle", ex);
        }
    }

    async Task AnimateDashboardAsync()
    {
        try
        {
            await Task.WhenAll(
                Ui.AnimateInAsync(TodayCard, 240, 0),
                Ui.AnimateInAsync(MonthCard, 240, 55),
                Ui.AnimateInAsync(WeekCard, 240, 110));
        }
        catch (Exception ex) { AppLog.Error("Home.Animation", ex); }
    }

    void OnStoreChanged() { if (_appeared) _ = LoadAsync(); }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appeared = true;
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;
        _animateDashboard = true;
        RefreshTitle();
        WidgetNav.TryGo();
        await LoadAsync();
        if (_animateDashboard)
        {
            _animateDashboard = false;
            _ = AnimateDashboardAsync();
        }
        await AskLanguageOnceAsync();
        await AutoBackupOnceAsync();
    }

    static bool _backupChecked;

    async Task AutoBackupOnceAsync()
    {
        try
        {
            if (_backupChecked || !Backup.IsDue) return;
            _backupChecked = true;
            if (Backup.HasLinked)
            {
                var r = await Backup.AutoIfDueAsync();
                if (r.Ran) Ui.Toast(r.Message);
            }
            else
            {
                var yes = await DisplayAlert(L.T("ব্যাকআপের সময় হয়েছে", "Backup is due"),
                    L.T("এখনই ব্যাকআপ নিয়ে জায়গা বেছে নেবেন?", "Back up now and choose where to save?"),
                    L.T("হ্যাঁ", "Yes"), L.T("পরে", "Later"));
                if (yes)
                {
                    var r = await Backup.RunAsync(true);
                    Ui.Toast(r.Message);
                }
            }
        }
        catch (Exception ex) { AppLog.Error("Home.AutoBackup", ex); }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appeared = false;
        Store.Changed -= OnStoreChanged;
    }

    async Task AskLanguageOnceAsync()
    {
        try
        {
            if (L.IsChosen) return;
            var pick = await DisplayActionSheet("ভাষা বাছুন / Choose language", null, null, "বাংলা", "English");
            if (pick == "English") await LangSwitch.ApplyAsync("en", goToSettings: false);
            else L.Set("bn");
        }
        catch { L.Set("bn"); }
    }

    async Task LoadAsync()
    {
        try
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            Period.Resolve(monthStart, out var pStart, out var pEnd);
            bool custom = Period.IsCustom;

            var from = pStart < today.AddDays(-6) ? pStart : today.AddDays(-6);
            var to = pEnd > today.AddDays(1) ? pEnd : today.AddDays(1);
            var all = await Store.GetRangeAsync(from, to);
            var month = all.Where(x => x.Date >= pStart && x.Date < pEnd).ToList();
            var todays = all.Where(x => x.Date.Date == today).ToList();
            var cats = await Store.GetCategoriesAsync();

            var todaySpent = todays.Sum(x => x.Amount);
            var monthSpent = month.Sum(x => x.Amount);
            var periodLimit = Period.PeriodLimit(pStart, pEnd);

            DateLabel.Text = today.ToString("dddd, dd MMMM yyyy", Fmt.Inv);
            TodaySpentLabel.Text = Fmt.Money0(todaySpent);
            MonthSpentLabel.Text = Fmt.Money0(monthSpent);

            ShowToday(Period.DailyLimitFor(today), todaySpent);
            MonthTitleLabel.Text = custom ? L.T("এই রেঞ্জে খরচ · ") + Period.Title(monthStart) : L.T("এই মাসে খরচ");
            RangeBtn.Text = custom ? L.T("📅 রেঞ্জ ও লিমিট বদলান") : L.T("📅 নিজে রেঞ্জ ঠিক করুন");
            ShowMonth(periodLimit, monthSpent, custom);
            BuildWeek(all, today);
            BuildLedger(pStart, pEnd, month, periodLimit, monthSpent, custom, today);

            BreakdownStack.Children.Clear();
            var names = cats.Select(c => c.Name).ToList();
            foreach (var extra in todays.Select(x => x.Cat).Distinct())
                if (!names.Contains(extra)) names.Add(extra);

            foreach (var name in names)
            {
                var sum = todays.Where(x => x.Cat == name).Sum(x => x.Amount);
                var row = new Grid
                {
                    ColumnSpacing = 10,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    }
                };
                row.Add(Ui.ItemBadge(name, 30), 0);
                row.Add(new Label { Text = name, FontSize = 16, TextColor = sum > 0 ? Ui.Ink : Ui.Muted, VerticalOptions = LayoutOptions.Center }, 1);
                row.Add(new Label { Text = Fmt.Money0(sum), FontSize = 16, TextColor = sum > 0 ? Ui.Ink : Ui.Muted, FontAttributes = sum > 0 ? FontAttributes.Bold : FontAttributes.None, VerticalOptions = LayoutOptions.Center }, 2);
                BreakdownStack.Add(row);
            }

            EntriesStack.Children.Clear();
            if (todays.Count == 0)
            {
                EntriesStack.Add(new Label { Text = L.T("আজ এখনো কোনো খরচ যোগ হয়নি"), TextColor = Ui.Muted });
            }
            else
            {
                foreach (var item in todays)
                    EntriesStack.Add(Ui.EntryRow(item, x => Ui.OpenEdit(this, x)));
            }
        }
        catch { }
    }

    void ShowToday(decimal limit, decimal spent)
    {
        if (limit <= 0)
        {
            TodayBarHost.Content = null;
            TodayLeftTitle.Text = L.T("দৈনিক লিমিট সেট করা নেই");
            TodayLimitInfo.Text = L.T("ট্যাপ করে সেট করুন");
            TodayLeftLabel.Text = "";
            return;
        }

        double frac = (double)(spent / limit);
        var fill = frac >= 1 ? OnDarkOver : frac >= 0.75 ? OnDarkWarn : Colors.White;
        TodayBarHost.Content = Ui.Bar(frac, fill, OnDarkTrack, 12);

        var left = limit - spent;
        if (left >= 0)
        {
            TodayLeftTitle.Text = L.T("আজ বাকি");
            TodayLeftLabel.Text = Fmt.Money0(left);
            TodayLeftLabel.TextColor = Colors.White;
        }
        else
        {
            TodayLeftTitle.Text = L.T("লিমিট ছাড়িয়েছে");
            TodayLeftLabel.Text = Fmt.Money0(-left);
            TodayLeftLabel.TextColor = OnDarkOverText;
        }
        TodayLimitInfo.Text = L.T("লিমিট ") + Fmt.Money0(limit) + " · " + Pct(frac) + L.T(" খরচ");
    }

    void ShowMonth(decimal limit, decimal spent, bool custom)
    {
        if (limit <= 0)
        {
            MonthBarHost.Content = null;
            MonthLeftTitle.Text = custom ? L.T("রেঞ্জের লিমিট") : L.T("মাসিক লিমিট");
            MonthLeftLabel.Text = L.T("সেট করুন");
            MonthLeftLabel.TextColor = Ui.Primary;
            MonthLimitInfo.Text = "";
            return;
        }

        double frac = (double)(spent / limit);
        var color = Ui.LimitColor(frac);
        MonthBarHost.Content = Ui.Bar(frac, color, null, 12);

        var left = limit - spent;
        if (left >= 0)
        {
            MonthLeftTitle.Text = custom ? L.T("রেঞ্জে বাকি") : L.T("মাসে বাকি");
            MonthLeftLabel.Text = Fmt.Money0(left);
            MonthLeftLabel.TextColor = Ui.Green;
        }
        else
        {
            MonthLeftTitle.Text = L.T("লিমিট ছাড়িয়েছে");
            MonthLeftLabel.Text = Fmt.Money0(-left);
            MonthLeftLabel.TextColor = Ui.Red;
        }
        MonthLimitInfo.Text = L.T("লিমিট ") + Fmt.Money0(limit) + " · " + Pct(frac) + L.T(" খরচ");
        MonthLimitInfo.TextColor = frac >= 0.75 ? color : Ui.Muted;
    }

    static string Pct(double frac) => Math.Round(frac * 100).ToString(Fmt.Inv) + "%";

    void BuildLedger(DateTime pStart, DateTime pEnd, List<Expense> month, decimal periodLimit, decimal spent, bool custom, DateTime today)
    {
        LedgerTitle.Text = custom ? L.T("রেঞ্জের দিনের হিসাব") : L.T("মাসের দিনের হিসাব");
        LedgerStack.Children.Clear();

        int totalDays = (pEnd - pStart).Days;
        int elapsed = Period.ElapsedDays(pStart, pEnd);

        if (periodLimit <= 0)
        {
            RingHost.Content = Ui.Ring(0, Ui.Primary, 124, 14);
            RingPct.Text = "—";
            RingPct.TextColor = Ui.Muted;
            RingSub.Text = L.T("লিমিট নেই");
            RingLimitLabel.Text = L.T("লিমিট সেট করা নেই");
            RingSpentLabel.Text = L.T("খরচ ") + Fmt.Money0(spent);
            RingLeftLabel.Text = "";
            RingDaysLabel.Text = elapsed.ToString(Fmt.Inv) + " / " + totalDays.ToString(Fmt.Inv) + L.T(" দিন কেটেছে");
            SoFarLabel.Text = custom
                ? L.T("রেঞ্জ ঠিক করার সময় লিমিট দিলে প্রতিদিনের জমা/বাড়তি এখানে দেখাবে")
                : L.T("সেটিংসে লিমিট দিলে প্রতিদিনের জমা/বাড়তি এখানে দেখাবে");
            SoFarLabel.TextColor = Ui.Muted;
            AdviceBox.IsVisible = false;
            LedgerCard.IsVisible = false;
            return;
        }

        LedgerCard.IsVisible = true;
        double frac = (double)(spent / periodLimit);
        var color = Ui.LimitColor(frac);
        RingHost.Content = Ui.Ring(frac, color, 124, 14);
        RingPct.Text = Pct(frac);
        RingPct.TextColor = color;
        RingSub.Text = L.T("খরচ হয়েছে");
        RingLimitLabel.Text = (custom ? L.T("রেঞ্জের লিমিট ") : L.T("মাসের লিমিট ")) + Fmt.Money0(periodLimit);
        RingSpentLabel.Text = L.T("খরচ ") + Fmt.Money0(spent);
        var left = periodLimit - spent;
        RingLeftLabel.Text = left >= 0 ? L.T("বাকি ") + Fmt.Money0(left) : L.T("বেশি ") + Fmt.Money0(-left);
        RingLeftLabel.TextColor = left >= 0 ? Ui.Green : Ui.Red;
        RingDaysLabel.Text = elapsed.ToString(Fmt.Inv) + " / " + totalDays.ToString(Fmt.Inv) + L.T(" দিন কেটেছে");

        var ledger = Period.Ledger(pStart, pEnd, month);
        if (ledger.Count == 0)
        {
            SoFarLabel.Text = L.T("এই সময় এখনো শুরু হয়নি");
            SoFarLabel.TextColor = Ui.Muted;
            AdviceBox.IsVisible = false;
            LedgerCard.IsVisible = false;
            return;
        }

        var net = ledger.Sum(x => x.Diff);
        if (net >= 0)
        {
            SoFarLabel.Text = L.T("এ পর্যন্ত ") + ledger.Count.ToString(Fmt.Inv) + L.T(" দিনে লিমিট থেকে ") + Fmt.Money0(net) + L.T(" কম খরচ হয়েছে");
            SoFarLabel.TextColor = Ui.Green;
        }
        else
        {
            SoFarLabel.Text = L.T("এ পর্যন্ত ") + ledger.Count.ToString(Fmt.Inv) + L.T(" দিনে লিমিটের চেয়ে ") + Fmt.Money0(-net) + L.T(" বেশি খরচ হয়েছে");
            SoFarLabel.TextColor = Ui.Red;
        }

        BuildAdvice(ledger, pEnd, periodLimit, spent, today);

        var rows = ledger.AsEnumerable().Reverse().Take(LedgerRows).ToList();
        var maxAbs = rows.Count == 0 ? 0 : rows.Max(r => Math.Abs(r.Diff));
        foreach (var r in rows) LedgerStack.Add(LedgerRow(r, maxAbs, today));
        MoreBtn.IsVisible = ledger.Count > LedgerRows;
    }

    void BuildAdvice(List<DayBal> ledger, DateTime pEnd, decimal periodLimit, decimal spent, DateTime today)
    {
        var todayRow = ledger.FirstOrDefault(x => x.Day == today);
        if (todayRow == null)
        {
            AdviceBox.IsVisible = false;
            return;
        }

        var before = ledger.Where(x => x.Day < today).ToList();
        var carry = before.Count == 0 ? 0m : before[before.Count - 1].Cum;
        var canToday = todayRow.Allowed + carry;

        string msg;
        Color bg;
        if (carry > 0)
        {
            msg = L.T("গতকাল পর্যন্ত ") + Fmt.Money0(carry) + L.T(" বেঁচেছে। আজ ") + Fmt.Money0(canToday) +
                  L.T(" পর্যন্ত খরচ করলেও গড় ঠিক থাকবে (আজের লিমিট ") + Fmt.Money0(todayRow.Allowed) + L.T(" + জমা ") + Fmt.Money0(carry) + L.T(")।", ").");
            bg = Ui.GreenSoft;
        }
        else if (carry < 0)
        {
            if (canToday > 0)
                msg = L.T("আগের দিনগুলোতে ") + Fmt.Money0(-carry) + L.T(" বেশি খরচ হয়েছে। আজ ") + Fmt.Money0(canToday) +
                      L.T(" এর মধ্যে থাকলে সেটা পুষিয়ে যাবে (আজের লিমিট ") + Fmt.Money0(todayRow.Allowed) + L.T(" − বাড়তি ") + Fmt.Money0(-carry) + L.T(")।", ").");
            else
                msg = L.T("আগের দিনগুলোতে ") + Fmt.Money0(-carry) + L.T(" বেশি খরচ হয়েছে। আজ যতটা পারেন কম খরচ করুন — আজের পুরো লিমিটেও এই বাড়তি পুষবে না।");
            bg = Ui.RedSoft;
        }
        else
        {
            msg = L.T("আজের লিমিট ") + Fmt.Money0(todayRow.Allowed) + L.T("। এর মধ্যে থাকলে গড় ঠিক থাকবে।");
            bg = Ui.PrimarySoft;
        }

        int daysLeft = (pEnd - today).Days;
        var spentBeforeToday = spent - todayRow.Spent;
        var remaining = periodLimit - spentBeforeToday;
        if (daysLeft > 0)
        {
            if (remaining > 0)
                msg += L.T("\nআজসহ বাকি ") + daysLeft.ToString(Fmt.Inv) + L.T(" দিনে দিনে গড়ে ") + Fmt.Money0(remaining / daysLeft) + L.T(" করে খরচ করতে পারবেন।");
            else
                msg += L.T("\nমোট লিমিট ইতিমধ্যে শেষ।");
        }

        AdviceLabel.Text = msg;
        AdviceLabel.TextColor = Ui.Ink;
        AdviceBox.BackgroundColor = bg;
        AdviceBox.IsVisible = true;
    }

    View LedgerRow(DayBal r, decimal maxAbs, DateTime today)
    {
        var day = r.Day == today ? L.T("আজ") : Fmt.DayMonth(r.Day);

        var leftBox = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        leftBox.Add(new Label { Text = day, FontSize = 13, FontAttributes = FontAttributes.Bold });
        leftBox.Add(new Label { Text = L.T("খরচ ") + Fmt.Money0(r.Spent), FontSize = 11, TextColor = Ui.Muted });

        var diff = r.Diff;
        var diffColor = diff >= 0 ? Ui.Green : Ui.Red;
        var rightBox = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        rightBox.Add(new Label
        {
            Text = diff >= 0 ? Fmt.Money0(diff) + L.T(" বাঁচল") : Fmt.Money0(-diff) + L.T(" বেশি"),
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = diffColor,
            HorizontalTextAlignment = TextAlignment.End
        });
        rightBox.Add(new Label
        {
            Text = r.Cum >= 0 ? L.T("মোট জমা ") + Fmt.Money0(r.Cum) : L.T("মোট ঘাটতি ") + Fmt.Money0(-r.Cum),
            FontSize = 11,
            TextColor = Ui.Muted,
            HorizontalTextAlignment = TextAlignment.End
        });

        var g = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(84)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(104))
            }
        };
        g.Add(leftBox, 0);
        g.Add(DivBar(diff, maxAbs), 1);
        g.Add(rightBox, 2);
        return g;
    }

    static View DivBar(decimal diff, decimal maxAbs)
    {
        double f = maxAbs > 0 ? Math.Min(1.0, (double)(Math.Abs(diff) / maxAbs)) : 0;
        if (f > 0 && f < 0.06) f = 0.06;

        var g = new Grid { HeightRequest = 12, ColumnSpacing = 0, VerticalOptions = LayoutOptions.Center };
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var track = new BoxView { Color = Ui.Gray, CornerRadius = 6 };
        Grid.SetColumnSpan(track, 2);
        g.Add(track);

        if (diff < 0 && f > 0)
        {
            var left = new Grid { ColumnSpacing = 0 };
            left.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - f, GridUnitType.Star)));
            left.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(f, GridUnitType.Star)));
            left.Add(new BoxView { Color = Ui.Red, CornerRadius = 6 }, 1);
            g.Add(left, 0);
        }
        else if (diff > 0 && f > 0)
        {
            var right = new Grid { ColumnSpacing = 0 };
            right.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(f, GridUnitType.Star)));
            right.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - f, GridUnitType.Star)));
            right.Add(new BoxView { Color = Ui.Green, CornerRadius = 6 }, 0);
            g.Add(right, 1);
        }

        var mid = new BoxView { Color = Ui.Muted, WidthRequest = 2, HeightRequest = 16, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        Grid.SetColumnSpan(mid, 2);
        g.Add(mid);
        return g;
    }

    void BuildWeek(List<Expense> all, DateTime today)
    {
        WeekChart.Children.Clear();
        WeekChart.ColumnDefinitions.Clear();

        var days = Enumerable.Range(0, 7).Select(i => today.AddDays(i - 6)).ToList();
        var sums = days.Select(d => all.Where(x => x.Date.Date == d).Sum(x => x.Amount)).ToList();
        var limits = days.Select(d => Period.DailyLimitFor(d)).ToList();
        var limit = limits[6];

        var max = sums.Max();
        if (limits.Max() > max) max = limits.Max();

        for (int i = 0; i < 7; i++)
        {
            WeekChart.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var s = sums[i];
            var isToday = days[i] == today;
            var over = limits[i] > 0 && s > limits[i];
            var color = over ? Ui.Red : isToday ? Ui.Primary : Ui.PrimaryLight;
            double h = s > 0 && max > 0 ? Math.Max(4, (double)(s / max) * BarMax) : 3;

            var col = new Grid
            {
                RowSpacing = 4,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(new GridLength(BarMax)),
                    new RowDefinition(GridLength.Auto)
                }
            };
            col.Add(new Label
            {
                Text = s > 0 ? Fmt.Short(s) : "",
                FontSize = 10,
                TextColor = over ? Ui.Red : Ui.Muted,
                HorizontalTextAlignment = TextAlignment.Center
            }, 0, 0);
            col.Add(new BoxView
            {
                Color = s > 0 ? color : Ui.Line,
                CornerRadius = 5,
                WidthRequest = 20,
                HeightRequest = h,
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Center
            }, 0, 1);
            col.Add(new Label
            {
                Text = Fmt.BnDayShort(days[i]),
                FontSize = 11,
                FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None,
                TextColor = isToday ? Ui.Primary : Ui.Muted,
                HorizontalTextAlignment = TextAlignment.Center
            }, 0, 2);

            WeekChart.Add(col, i, 0);
        }

        var total = sums.Sum();
        var hint = L.T("৭ দিনে মোট ") + Fmt.Money0(total) + L.T(" · দিনে গড় ") + Fmt.Money0(total / 7);
        hint += limit > 0
            ? L.T("\nলাল বার = ওই দিন দৈনিক লিমিট (") + Fmt.Money0(limit) + L.T(") ছাড়িয়েছে")
            : L.T("\nদৈনিক লিমিট সেট করলে লিমিট ছাড়ানো দিন লাল দেখাবে");
        WeekHint.Text = hint;
    }

    async void OnTodayCardTapped(object? sender, TappedEventArgs e)
    {
        try { Ui.OpenModal(this, new LimitsPage()); await Task.CompletedTask; } catch { }
    }

    async void OnMonthCardTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (Period.IsCustom) await OpenRangeEditor();
            else Ui.OpenModal(this, new LimitsPage());
        }
        catch { }
    }

    async void OnRangeClicked(object? sender, EventArgs e)
    {
        try { await OpenRangeEditor(); } catch { }
    }

    static async Task OpenRangeEditor()
    {
        Ui.ReturnRoute = "home";
        Ui.PendingOpenRange = true;
        await Ui.GoTo("report");
    }

    async void OnMoreClicked(object? sender, EventArgs e)
    {
        try { await Ui.GoTo("report"); } catch { }
    }

    async void OnAddClicked(object? sender, EventArgs e)
    {
        try { await Ui.GoTo("add"); } catch { }
    }
}
