using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public class CalculatorPage : ContentPage, IHostActivatable
{
    string _expr = "";
    bool _justEvaled;

    readonly Label _exprLbl = new() { FontSize = 20, TextColor = Ui.Muted, HorizontalTextAlignment = TextAlignment.End, LineBreakMode = LineBreakMode.HeadTruncation, MaxLines = 1 };
    readonly Label _resLbl = new() { FontSize = 50, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End, LineBreakMode = LineBreakMode.HeadTruncation, MaxLines = 1 };

    readonly Picker _catPick = new() { TextColor = Ui.Ink, FontSize = 16 };
    readonly Picker _optPick = new() { TextColor = Ui.Ink, FontSize = 16 };
    readonly Border _optBox;
    readonly Entry _note = new() { FontSize = 16, MaxLength = 100 };
    readonly DatePicker _date = new() { Format = "dd MMM yyyy", TextColor = Ui.Ink };
    readonly Button _addBtn = new() { HeightRequest = 52, FontSize = 16 };
    readonly Button _offerBtn = new() { HeightRequest = 52, FontSize = 16, IsVisible = false };
    readonly Button _cancelBtn = new() { HeightRequest = 48, FontSize = 15, StyleClass = new[] { "Outline" } };
    Border _formCard = null!;
    ScrollView _formScroll = null!;
    Grid _keys = null!;
    bool _formOpen;
    readonly Label _dayHint = new() { FontSize = 12, TextColor = Ui.Muted, VerticalOptions = LayoutOptions.Center };

    List<ExpenseCategory> _cats = new();
    List<Choice> _choices = new();
    bool _shown;
    bool _busy;

    public CalculatorPage()
    {
        Title = L.T("ক্যালকুলেটর", "Calculator");

        var disp = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.End };
        disp.Add(_exprLbl);
        disp.Add(_resLbl);
        var dispCard = new Border
        {
            Content = disp,
            Padding = new Thickness(18, 14),
            BackgroundColor = Ui.Surface,
            Stroke = Ui.Line,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) }
        };

        var keys = new Grid { ColumnSpacing = 6, RowSpacing = 5, VerticalOptions = LayoutOptions.Fill };
        _keys = keys;
        for (int c = 0; c < 4; c++) keys.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (int r = 0; r < 5; r++) keys.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        string[,] layout =
        {
            { "C", "⌫", "%", "÷" },
            { "7", "8", "9", "×" },
            { "4", "5", "6", "−" },
            { "1", "2", "3", "+" },
            { "0", "00", ".", "=" }
        };
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 4; c++)
                keys.Add(Key(layout[r, c]), c, r);

        _optBox = FUi.Field(_optPick);
        _note.Placeholder = L.T("নোট (ঐচ্ছিক)", "Note (optional)");
        _cancelBtn.Text = L.T("বাতিল", "Cancel");
        _cancelBtn.Clicked += (_, _) => { _formOpen = false; Refresh(); };
        _offerBtn.Clicked += (_, _) => { _formOpen = true; Refresh(); };
        _catPick.Title = L.T("ক্যাটাগরি বাছুন", "Choose category");
        _optPick.Title = L.T("অপশন বাছুন", "Choose option");
        _date.MaximumDate = DateTime.Today;
        _date.MinimumDate = new DateTime(2000, 1, 1);
        _date.Date = DateTime.Today;
        _date.DateSelected += (_, _) => UpdateDayHint();
        _catPick.SelectedIndexChanged += (_, _) => { if (!_busy) BuildOptions(); };
        _addBtn.Clicked += async (_, _) => await AddExpenseAsync();

        var add = new VerticalStackLayout { Spacing = 8 };
        add.Add(new Label { Text = L.T("🧾 এই টাকা খরচে যোগ করুন", "🧾 Add this amount as an expense"), FontSize = 17, FontAttributes = FontAttributes.Bold });
        add.Add(FUi.Field(_catPick));
        add.Add(_optBox);
        add.Add(FUi.Field(_note));
        var dRow = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        dRow.Add(FUi.Field(_date), 0);
        dRow.Add(_dayHint, 1);
        add.Add(dRow);
        add.Add(_addBtn);
        add.Add(_cancelBtn);
        _formCard = FUi.Card(add, Ui.Primary.WithAlpha(0.45f), 14);
        _formCard.IsVisible = false;

        _formScroll = new ScrollView { Content = _formCard, IsVisible = false };
        var root = new Grid
        {
            Padding = new Thickness(14, 10, 14, 8),
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition(new GridLength(2.6, GridUnitType.Star)),
                new RowDefinition(new GridLength(5, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto)
            }
        };
        root.Add(dispCard, 0, 0);
        root.Add(keys, 0, 1);
        root.Add(_formScroll, 0, 1);
        root.Add(_offerBtn, 0, 2);
        Content = root;

        Refresh();
        UpdateDayHint();
        RootChrome.Attach(this);
    }

    void OnStoreChanged() { if (_shown) _ = LoadCatsAsync(); }

    public async Task ActivateForHostAsync() => await ActivateCoreAsync();

    public void DeactivateForHost() { OnDisappearing(); }

    async Task ActivateCoreAsync()
    {
        base.OnAppearing();
        _shown = true;
        _date.MaximumDate = DateTime.Today;
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;
        await LoadCatsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _shown = false;
        Store.Changed -= OnStoreChanged;
    }

    View Key(string k)
    {
        bool isOp = k is "÷" or "×" or "−" or "+" or "%";
        var b = new Button
        {
            Text = k,
            MinimumHeightRequest = 36,
            FontSize = 26,
            CornerRadius = 14,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(0)
        };
        if (k == "=") { b.BackgroundColor = Ui.Primary; b.TextColor = Colors.White; }
        else if (k == "C") { b.BackgroundColor = Ui.RedSoft; b.TextColor = Ui.Red; }
        else if (isOp || k == "⌫") { b.BackgroundColor = Ui.PrimarySoft; b.TextColor = Ui.PrimaryOn; }
        else { b.BackgroundColor = Ui.Surface; b.TextColor = Ui.Ink; b.BorderColor = Ui.Line; b.BorderWidth = 1; }
        b.Clicked += async (_, _) =>
        {
            await Ui.TapPulseAsync(b);
            Press(k);
        };
        return b;
    }

    static bool IsOp(char c) => c is '+' or '−' or '×' or '÷';

    void Press(string k)
    {
        switch (k)
        {
            case "C":
                _expr = ""; _justEvaled = false; break;

            case "⌫":
                if (_expr.Length > 0) _expr = _expr.Substring(0, _expr.Length - 1);
                _justEvaled = false;
                break;

            case "=":
            {
                var v = Evaluate(_expr, out var ok);
                if (ok && _expr.Length > 0) { _expr = Format(v, false); _justEvaled = true; }
                break;
            }

            case "+": case "−": case "×": case "÷":
                _justEvaled = false;
                if (_expr.Length == 0) { if (k == "−") _expr = "−"; break; }
                if (_expr == "−") break;
                if (IsOp(_expr[^1]))
                {

                    _expr = _expr.Substring(0, _expr.Length - 1) + k;
                }
                else _expr += k;
                break;

            case "%":
                if (_expr.Length > 0 && char.IsDigit(_expr[^1])) _expr += "%";
                _justEvaled = false;
                break;

            case ".":
                if (_justEvaled) { _expr = "0."; _justEvaled = false; break; }
                {
                    var cur = CurrentNumber();
                    if (cur.Contains('.')) break;
                    _expr += (cur.Length == 0 ? "0." : ".");
                }
                break;

            default:
                if (_justEvaled) { _expr = ""; _justEvaled = false; }
                if (_expr.EndsWith("%")) _expr += "×";
                if (CurrentNumber().Length >= 12) break;
                if (k == "00" && CurrentNumber().Length == 0) k = "0";
                if (CurrentNumber() == "0" && k != "0" && k != "00") _expr = _expr.Substring(0, _expr.Length - 1) + k;
                else if (CurrentNumber() == "0" && (k == "0" || k == "00")) { }
                else _expr += k;
                break;
        }
        Refresh();
    }

    string CurrentNumber()
    {
        int i = _expr.Length;
        while (i > 0 && (char.IsDigit(_expr[i - 1]) || _expr[i - 1] == '.')) i--;
        return _expr.Substring(i);
    }

    void Refresh()
    {
        _exprLbl.Text = _expr.Length == 0 ? " " : _expr;
        var v = Evaluate(_expr, out var ok);
        if (_expr.Length == 0) { _resLbl.Text = "0"; _resLbl.TextColor = Ui.Muted; }
        else if (!ok) { _resLbl.Text = _expr == "−" ? "0" : L.T("ভুল", "Error"); _resLbl.TextColor = Ui.Red; }
        else { _resLbl.Text = Format(v, true); _resLbl.TextColor = Ui.Ink; }
        UpdateAddArea(ok ? v : (decimal?)null);
    }

    static string Format(decimal v, bool thousands)
    {
        v = Math.Round(v, 2, MidpointRounding.AwayFromZero);
        return thousands ? v.ToString("#,##0.##", Fmt.Inv) : v.ToString("0.##", Fmt.Inv);
    }

    static decimal Evaluate(string expr, out bool ok)
    {
        ok = false;
        if (string.IsNullOrEmpty(expr)) return 0;
        try
        {
            var s = expr;
            while (s.Length > 0 && IsOp(s[^1])) s = s.Substring(0, s.Length - 1);
            if (s.Length == 0 || s == "−") return 0;

            var items = new List<(char Op, decimal Val, bool Pct)>();
            char op = '+';
            int i = 0;
            if (s[0] == '−') { op = '−'; i = 1; }
            while (i < s.Length)
            {
                int st = i;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                if (st == i) return 0;
                if (!decimal.TryParse(s.Substring(st, i - st), System.Globalization.NumberStyles.Number, Fmt.Inv, out var num)) return 0;
                bool pct = false;
                if (i < s.Length && s[i] == '%') { pct = true; i++; }
                items.Add((op, num, pct));
                if (i < s.Length)
                {
                    if (!IsOp(s[i])) return 0;
                    op = s[i]; i++;
                }
            }
            if (items.Count == 0) return 0;

            var terms = new List<(bool Neg, decimal Val, bool PurePct)>();
            int k = 0;
            while (k < items.Count)
            {
                bool neg = items[k].Op == '−';
                decimal val = items[k].Val;
                bool pure = items[k].Pct;
                int j = k + 1;
                bool hasMul = j < items.Count && (items[j].Op == '×' || items[j].Op == '÷');
                if (hasMul && items[k].Pct) { val /= 100; pure = false; }
                while (j < items.Count && (items[j].Op == '×' || items[j].Op == '÷'))
                {
                    var f = items[j].Pct ? items[j].Val / 100 : items[j].Val;
                    if (items[j].Op == '×') val *= f;
                    else
                    {
                        if (f == 0) return 0;
                        val /= f;
                    }
                    pure = false;
                    j++;
                }
                terms.Add((neg, val, pure));
                k = j;
            }

            decimal total = 0;
            for (int t = 0; t < terms.Count; t++)
            {
                var (neg, val, pure) = terms[t];
                decimal v = val;
                if (pure)
                    v = t == 0 ? val / 100 : total * val / 100;
                total += neg ? -v : v;
            }
            ok = true;
            return total;
        }
        catch
        {
            ok = false;
            return 0;
        }
    }

    void UpdateDayHint() => _dayHint.Text = Fmt.DayHint(_date.Date);

    void UpdateAddArea(decimal? v)
    {
        bool can = _justEvaled && v != null && v > 0;
        if (!can) _formOpen = false;
        _offerBtn.IsVisible = can && !_formOpen;
        _formCard.IsVisible = can && _formOpen;
        _formScroll.IsVisible = can && _formOpen;
        _keys.IsVisible = !(can && _formOpen);
        if (can)
        {
            var t = Fmt.Money0(Math.Round(v!.Value, 2));
            _offerBtn.Text = L.T("➕ খরচে যোগ করুন  ", "➕ Add to expenses  ") + t;
            _addBtn.Text = L.T("✔ যোগ করুন  ", "✔ Add  ") + t;
        }
    }

    async Task LoadCatsAsync()
    {
        try
        {
            _cats = await Store.GetCategoriesAsync();
            _choices = await Store.GetChoicesAsync();
            _busy = true;
            var prev = _catPick.SelectedIndex >= 0 && _catPick.SelectedIndex < _cats.Count ? _cats[_catPick.SelectedIndex].Name
                : Preferences.Default.Get("calc_cat", "");
            _catPick.ItemsSource = _cats.Select(c => c.Icon + " " + c.Name).ToList();
            var idx = _cats.FindIndex(c => c.Name == prev);
            _catPick.SelectedIndex = idx >= 0 ? idx : (_cats.Count > 0 ? 0 : -1);
            _busy = false;
            BuildOptions();
        }
        catch { _busy = false; }
    }

    ExpenseCategory? SelCat => _catPick.SelectedIndex >= 0 && _catPick.SelectedIndex < _cats.Count ? _cats[_catPick.SelectedIndex] : null;

    void BuildOptions()
    {
        var cat = SelCat;
        if (cat == null) { _optBox.IsVisible = false; return; }
        Preferences.Default.Set("calc_cat", cat.Name);

        if (cat.IsFreeText)
        {
            _optBox.IsVisible = false;
            _note.Placeholder = L.T("নোট (ঐচ্ছিক)", "Note (optional)");
            return;
        }
        _note.Placeholder = L.T("নোট (ঐচ্ছিক)", "Note (optional)");
        var names = _choices.Where(c => c.CategoryId == cat.Id).Select(c => c.Name).ToList();
        _optBox.IsVisible = names.Count > 0;
        _busy = true;
        _optPick.ItemsSource = names;
        _optPick.SelectedIndex = names.Count > 0 ? 0 : -1;
        _busy = false;
    }

    async Task AddExpenseAsync()
    {
        try
        {
            var v = Evaluate(_expr, out var ok);
            v = Math.Round(v, 2, MidpointRounding.AwayFromZero);
            if (!ok || v <= 0)
            {
                Ui.Toast(L.T("আগে হিসাব করে টাকার অঙ্ক বের করুন", "Calculate an amount first"));
                return;
            }
            var cat = SelCat;
            if (cat == null)
            {
                Ui.Toast(L.T("ক্যাটাগরি বাছুন", "Choose a category"));
                return;
            }

            string item = cat.Name;
            if (!cat.IsFreeText && _optBox.IsVisible && _optPick.SelectedIndex >= 0)
                item = _optPick.SelectedItem?.ToString() ?? cat.Name;

            var day = _date.Date.Date;
            var isToday = day == DateTime.Today;
            await Store.AddAsync(new Expense
            {
                Date = isToday ? DateTime.Now : day + DateTime.Now.TimeOfDay,
                Item = item,
                Category = cat.Name,
                Amount = v,
                Note = (_note.Text ?? "").Trim()
            });

            var msg = "✔ " + (item == cat.Name ? cat.Name : cat.Name + " · " + item) + ": " + Fmt.Money(v) + L.T(" যোগ হয়েছে", " added");
            if (!isToday) msg += " (" + day.ToString("dd MMM", Fmt.Inv) + ")";
            Ui.Toast(msg);

            _note.Text = "";
            _expr = "";
            _justEvaled = false;
            _formOpen = false;
            Refresh();
        }
        catch { }
    }
}
