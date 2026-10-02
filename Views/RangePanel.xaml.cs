using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

public partial class RangePanel : ContentView
{
    bool _sync;
    bool _editing;
    DateTime _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    bool _canNext;

    public event Action<int>? MonthStep;

    public RangePanel()
    {
        InitializeComponent();
        Render();
    }

    public void Refresh(DateTime month, bool canNext)
    {
        _month = month;
        _canNext = canNext;
        Render();
    }

    void Render()
    {
        try
        {
            var custom = Period.IsCustom;
            var main = custom ? Ui.RangeColor : Ui.Primary;
            var dark = custom ? Ui.RangeDark : Ui.PrimaryDark;
            var soft = Color.FromArgb(custom ? "#DCD6F5" : "#CFF3EE");

            CurrentCard.Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(custom ? Ui.RangeColor : Ui.PrimaryBright, 0f),
                    new GradientStop(dark, 1f)
                },
                new Point(0, 0), new Point(1, 1));

            ModeLabel.TextColor = soft;
            InfoLabel.TextColor = soft;
            ModeLabel.Text = custom ? L.T("📅 রেঞ্জ অনুযায়ী দেখছেন") : L.T("🗓 মাস অনুযায়ী দেখছেন");

            PrevBtn.IsVisible = !custom;
            NextBtn.IsVisible = !custom;
            NextBtn.IsEnabled = _canNext;
            NextBtn.Opacity = _canNext ? 1 : 0.3;
            Grid.SetColumn(TitleLabel, custom ? 0 : 1);
            Grid.SetColumnSpan(TitleLabel, custom ? 3 : 1);

            if (custom)
            {
                var end = Period.CustomEnd < Period.CustomStart ? Period.CustomStart : Period.CustomEnd;
                var lim = Period.RangeLimit;
                TitleLabel.Text = Fmt.RangeTitle(Period.CustomStart, end);
                InfoLabel.Text = Period.RangeDays.ToString(Fmt.Inv) + L.T(" দিন · ") +
                    (lim > 0
                        ? L.T("লিমিট ") + Fmt.Money0(lim) + L.T(" · দিনে ") + Fmt.Money0(lim / Period.RangeDays)
                        : L.T("লিমিট সেট করা নেই"));
            }
            else
            {
                TitleLabel.Text = Fmt.MonthTitle(_month);
                var days = DateTime.DaysInMonth(_month.Year, _month.Month);
                var ml = AppSettings.MonthlyLimit;
                InfoLabel.Text = days.ToString(Fmt.Inv) + L.T(" দিনের মাস") +
                    (ml > 0 ? L.T(" · মাসিক লিমিট ") + Fmt.Money0(ml) : "");
            }

            Style(MonthBtn, Ui.Primary, !custom, !custom ? L.T("✓ মাস অনুযায়ী") : L.T("🗓 মাস অনুযায়ী"));
            Style(RangeBtn, Ui.RangeColor, custom, custom ? L.T("✎ রেঞ্জ এডিট করুন") : L.T("📅 রেঞ্জ অনুযায়ী"));

            FormCard.IsVisible = _editing;
            FormTitle.Text = custom ? L.T("রেঞ্জ এডিট করুন") : L.T("রেঞ্জ ঠিক করুন");
        }
        catch { }
    }

    static void Style(Button b, Color color, bool selected, string text)
    {
        b.Text = text;
        b.BackgroundColor = selected ? color : Ui.Surface;
        b.TextColor = selected ? Colors.White : color;
        b.BorderColor = color;
    }

    void OnPrev(object? sender, EventArgs e) => MonthStep?.Invoke(-1);

    void OnNext(object? sender, EventArgs e) => MonthStep?.Invoke(1);

    public void OpenEditor()
    {
        _sync = true;
        try
        {
            var s = Period.CustomStart;
            var e = Period.CustomEnd < s ? s : Period.CustomEnd;
            FromPick.Date = s;
            ToPick.Date = e;
            LimitEntry.Text = Period.RangeLimit > 0 ? Period.RangeLimit.ToString("0.##", Fmt.Inv) : "";
        }
        catch { }
        finally { _sync = false; }

        _editing = true;
        UpdatePreview();
        Render();
    }

    void OnEdit(object? sender, EventArgs e) => OpenEditor();

    void OnCancel(object? sender, EventArgs e)
    {
        _editing = false;
        Ui.ReturnRoute = null;
        Render();
    }

    void OnMonthMode(object? sender, EventArgs e)
    {
        var wasCustom = Period.IsCustom;
        _editing = false;
        Ui.ReturnRoute = null;

        if (wasCustom)
        {
            Period.IsCustom = false;
            Render();
            Store.RaiseChanged();
            Ui.Toast(L.T("মাস অনুযায়ী হিসাব চালু হয়েছে"));
        }
        else
        {
            Render();
        }
    }

    void OnFormChanged(object? sender, DateChangedEventArgs e)
    {
        if (_sync) return;

        _sync = true;
        try
        {
            if (ToPick.Date < FromPick.Date)
            {
                if (ReferenceEquals(sender, FromPick)) ToPick.Date = FromPick.Date;
                else FromPick.Date = ToPick.Date;
            }
        }
        catch { }
        finally { _sync = false; }
        UpdatePreview();
    }

    void OnLimitChanged(object? sender, TextChangedEventArgs e) => UpdatePreview();

    void UpdatePreview()
    {
        try
        {
            int days = (ToPick.Date.Date - FromPick.Date.Date).Days + 1;
            if (days < 1) days = 1;
            var txt = days.ToString(Fmt.Inv) + L.T(" দিনের রেঞ্জ");
            if (Fmt.TryAmount(LimitEntry.Text, out var lim) && lim > 0)
                txt += L.T(" · দিনে গড়ে ") + Fmt.Money0(lim / days);
            else
                txt += L.T(" · লিমিট না দিলে শুধু খরচ দেখাবে");
            PreviewLabel.Text = txt;
        }
        catch { }
    }

    async void OnSave(object? sender, EventArgs e)
    {
        try
        {
            var from = FromPick.Date.Date;
            var to = ToPick.Date.Date;
            if (to < from)
            {
                Ui.Toast(L.T("শেষ তারিখ শুরুর আগে হতে পারে না"));
                return;
            }

            decimal lim = 0;
            if (!string.IsNullOrWhiteSpace(LimitEntry.Text) && !Fmt.TryAmount(LimitEntry.Text, out lim))
            {
                Ui.Toast(L.T("লিমিটের অঙ্ক ঠিক নয়"));
                return;
            }

            Period.CustomStart = from;
            Period.CustomEnd = to;
            Period.RangeLimit = lim;
            Period.IsCustom = true;
            _editing = false;
            Render();
            Store.RaiseChanged();

            Ui.Toast(L.T("✔ রেঞ্জ সেট হয়েছে: ") + Fmt.RangeTitle(from, to) + (lim > 0 ? L.T(" · লিমিট ") + Fmt.Money0(lim) : ""));

            var back = Ui.ReturnRoute;
            Ui.ReturnRoute = null;
            if (!string.IsNullOrEmpty(back)) await Ui.GoTo(back);
        }
        catch { }
    }
}
