using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

// Small helpers for
static class FUi
{
    public static Border Card(View content, Color? stroke = null, double pad = 14) => new Border
    {
        Content = content,
        Padding = new Thickness(pad, 12),
        BackgroundColor = Ui.Surface,
        Stroke = stroke ?? Ui.Line,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
    };

    public static Label Muted(string t, double size = 13) => new Label { Text = t, FontSize = size, TextColor = Ui.Muted };

    public static Border Field(View v) => new Border
    {
        Content = v,
        Padding = new Thickness(12, 0),
        BackgroundColor = Ui.FieldBg,
        Stroke = Ui.Line,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }
    };

    public static Button Btn(string text, Color color, bool filled, Action onClick, double h = 48)
    {
        var b = new Button
        {
            Text = text,
            HeightRequest = h,
            FontSize = 15,
            CornerRadius = 16,
            BorderWidth = 1.5,
            BorderColor = color,
            BackgroundColor = filled ? color : Ui.Surface,
            TextColor = filled ? Colors.White : color
        };
        b.Clicked += (_, _) => onClick();
        return b;
    }

    public static string Money(decimal v) => Fmt.Money0(v);

    // Bordered box like
    public static View Box(View content) => new Border
    {
        Content = content,
        Padding = new Thickness(10, 10),
        BackgroundColor = Colors.Transparent,
        Stroke = Ui.Primary.WithAlpha(0.55f),
        StrokeThickness = 1.5,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) }
    };

    // Selectable row: icon
    public static View Option(string icon, string title, string? sub, bool selected, Func<Task> onTap)
    {
        var col = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        col.Add(new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold });
        if (!string.IsNullOrEmpty(sub)) col.Add(new Label { Text = sub, FontSize = 12, TextColor = Ui.Muted });

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
        g.Add(Ui.Badge(Ui.Primary, icon, 40), 0);
        g.Add(col, 1);
        g.Add(new Label { Text = selected ? "✔" : "", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Ui.Primary, VerticalOptions = LayoutOptions.Center }, 2);

        var b = new Border
        {
            Content = g,
            Padding = new Thickness(12, 10),
            BackgroundColor = selected ? Ui.PrimarySoft : Ui.Surface,
            Stroke = selected ? Ui.Primary : Ui.Line,
            StrokeThickness = selected ? 1.5 : 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await onTap()) });
        return b;
    }

    // Fieldset-like box: bordered
    public static View Fieldset(string legend, View content)
    {
        var box = new Border
        {
            Content = content,
            Padding = new Thickness(10, 22, 10, 12),
            Margin = new Thickness(0, 13, 0, 0),
            BackgroundColor = Colors.Transparent,
            Stroke = Ui.Primary.WithAlpha(0.55f),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) }
        };
        var lg = new Label
        {
            Text = legend,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ui.Primary,
            BackgroundColor = Ui.PageBg,
            Padding = new Thickness(8, 0),
            HeightRequest = 26,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(18, 0, 0, 0)
        };
        var g = new Grid();
        g.Add(box);
        g.Add(lg);
        return g;
    }
}

// Hint
// Loan list (iOwe
// Hint
public class LoanPage : ContentPage
{
    readonly bool _iOwe;
    readonly VerticalStackLayout _list = new() { Spacing = 10 };
    readonly Label _totalLbl = new() { FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Colors.White };
    readonly Label _subLbl = new() { FontSize = 13, TextColor = Color.FromArgb("#F0F0F0") };
    readonly Border _head = new() { StrokeThickness = 0, Padding = new Thickness(20, 16), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) } };

    public LoanPage(bool iOwe)
    {
        _iOwe = iOwe;
        Title = iOwe ? L.T("ঋণ (আমি দেব)", "Debts (I owe)") : L.T("পাওনা (আমি পাব)", "To receive (owed to me)");
        ToolbarItems.Add(new ToolbarItem { Text = L.T("বন্ধ করুন", "Close"), Command = new Command(async () => await CloseAsync()) });

        var c1 = iOwe ? Color.FromArgb("#E57373") : Color.FromArgb("#43A047");
        var c2 = iOwe ? Color.FromArgb("#B71C1C") : Color.FromArgb("#1B5E20");
        _head.Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(c1, 0f), new GradientStop(c2, 1f) }, new Point(0, 0), new Point(1, 1));
        var hv = new VerticalStackLayout { Spacing = 2 };
        hv.Add(new Label { Text = iOwe ? L.T("মোট কত টাকা ঋণ আছে", "Total debt you still owe") : L.T("মোট কত টাকা পাবেন", "Total you still have to receive"), FontSize = 14, TextColor = Colors.White });
        hv.Add(_totalLbl);
        hv.Add(_subLbl);
        _head.Content = hv;

        var add = FUi.Btn(iOwe ? L.T("+ নতুন ঋণ যোগ করুন", "+ Add a debt") : L.T("+ নতুন পাওনা যোগ করুন", "+ Add money to receive"), c2, true, () => OpenEdit(null), 52);

        var root = new VerticalStackLayout { Padding = new Thickness(16, 14, 16, 30), Spacing = 12 };
        root.Add(_head);
        root.Add(add);
        root.Add(_list);
        Content = new ScrollView { Content = root };
    }

    bool _shown;
    void OnChanged() { if (_shown) _ = LoadAsync(); }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _shown = true;
        Store.Changed -= OnChanged;
        Store.Changed += OnChanged;
        await LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _shown = false;
        Store.Changed -= OnChanged;
    }

    // Back closes page
    protected override bool OnBackButtonPressed()
    {
        if (Navigation.NavigationStack.Count > 1) return base.OnBackButtonPressed();
        _ = CloseAsync();
        return true;
    }

    async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    void OpenEdit(Loan? l)
    {
        try { _ = Navigation.PushAsync(new LoanEditPage(_iOwe, l)); } catch { }
    }

    async Task LoadAsync()
    {
        try
        {
            var loans = await Store.GetLoansAsync(_iOwe);
            var open = loans.Where(x => !x.Settled).ToList();
            _totalLbl.Text = FUi.Money(open.Sum(x => x.Remaining));
            var paid = loans.Sum(x => x.Paid);
            _subLbl.Text = open.Count.ToString(Fmt.Inv) + L.T(" জনের কাছে বাকি", _iOwe ? " people to pay" : " people owe you")
                           + (paid > 0 ? L.T(" · এ পর্যন্ত ", " · so far ") + FUi.Money(paid) + (_iOwe ? L.T(" শোধ", " paid") : L.T(" পেয়েছেন", " received")) : "");
            if (_iOwe) _subLbl.Text = open.Count.ToString(Fmt.Inv) + L.T(" জনকে দিতে হবে", " people to pay")
                           + (paid > 0 ? L.T(" · এ পর্যন্ত ", " · so far ") + FUi.Money(paid) + L.T(" শোধ", " paid") : "");

            _list.Children.Clear();
            if (loans.Count == 0)
            {
                _list.Add(new Label
                {
                    Text = _iOwe ? L.T("কোনো ঋণ যোগ করা নেই", "No debts added") : L.T("কোনো পাওনা যোগ করা নেই", "Nothing to receive added"),
                    TextColor = Ui.Muted,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20)
                });
                return;
            }
            _list.Add(FUi.Muted(L.T("এন্ট্রিতে ট্যাপ করে এডিট, টাকা জমা বা মুছুন", "Tap an entry to edit, record a payment or delete")));
            foreach (var l in loans) _list.Add(Row(l));
        }
        catch { }
    }

    View Row(Loan l)
    {
        var main = _iOwe ? Ui.Red : Ui.Green;
        var color = l.Settled ? Ui.Muted : main;

        var mid = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        mid.Add(new Label { Text = l.Person, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = l.Settled ? Ui.Muted : Ui.Ink, LineBreakMode = LineBreakMode.TailTruncation });
        mid.Add(FUi.Muted(L.T("মোট ", "Total ") + FUi.Money(l.Amount) + (l.Paid > 0 ? (_iOwe ? L.T(" · শোধ ", " · paid ") : L.T(" · পেয়েছেন ", " · received ")) + FUi.Money(l.Paid) : ""), 12));
        if (l.HasDue)
        {
            var overdue = !l.Settled && l.DueDate.Date < DateTime.Today;
            var dueTxt = (_iOwe ? L.T("📅 দেওয়ার তারিখ: ", "📅 Pay by: ") : L.T("📅 পাওয়ার তারিখ: ", "📅 Expected: ")) + l.DueDate.ToString("dd MMM yyyy", Fmt.Inv);
            if (overdue) dueTxt += L.T(" (সময় পেরিয়েছে)", " (overdue)");
            mid.Add(new Label { Text = dueTxt, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = overdue ? Ui.Red : Ui.Muted });
        }
        if (!string.IsNullOrWhiteSpace(l.Note)) mid.Add(new Label { Text = l.Note, FontSize = 12, TextColor = Ui.Ink, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation });

        var right = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        right.Add(new Label { Text = l.Settled ? L.T("✔ শেষ", "✔ Settled") : FUi.Money(l.Remaining), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = color, HorizontalTextAlignment = TextAlignment.End });
        if (!l.Settled) right.Add(new Label { Text = L.T("বাকি", "remaining"), FontSize = 11, TextColor = Ui.Muted, HorizontalTextAlignment = TextAlignment.End });

        var g = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        g.Add(mid, 0);
        g.Add(right, 1);

        var box = new VerticalStackLayout { Spacing = 8 };
        box.Add(g);
        double frac = l.Amount > 0 ? (double)(l.Paid / l.Amount) : 0;
        if (l.Paid > 0) box.Add(Ui.Bar(frac, main, null, 8));

        var card = FUi.Card(box, l.Settled ? Ui.Line : main.WithAlpha(0.45f));
        if (l.Settled) card.Opacity = 0.7;
        var loan = l;
        card.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => OpenEdit(loan)) });
        return card;
    }
}

// Hint
// Loan add /
// Hint
public class LoanEditPage : ContentPage
{
    readonly bool _iOwe;
    readonly Loan _loan;
    readonly Entry _person = new() { MaxLength = 50 };
    readonly Entry _amount = new() { Keyboard = Keyboard.Numeric, FontSize = 22, FontAttributes = FontAttributes.Bold };
    readonly Entry _paid = new() { Keyboard = Keyboard.Numeric };
    readonly Entry _pay = new() { Keyboard = Keyboard.Numeric, HorizontalTextAlignment = TextAlignment.End };
    readonly Switch _hasDue = new();
    readonly DatePicker _due = new();
    readonly Editor _note = new() { AutoSize = EditorAutoSizeOption.TextChanges, MinimumHeightRequest = 48, MaxLength = 200 };
    readonly Label _remLbl = new() { FontAttributes = FontAttributes.Bold, FontSize = 14 };

    public LoanEditPage(bool iOwe, Loan? existing)
    {
        _iOwe = iOwe;
        bool isNew = existing == null;
        _loan = existing ?? new Loan { IOwe = iOwe, Created = DateTime.Now, DueDate = DateTime.Today.AddDays(7) };

        Title = isNew
            ? (iOwe ? L.T("নতুন ঋণ", "New debt") : L.T("নতুন পাওনা", "New receivable"))
            : L.T("এডিট করুন", "Edit");

        _person.Placeholder = iOwe ? L.T("কার কাছ থেকে নিয়েছেন", "Who did you borrow from") : L.T("কাকে দিয়েছেন", "Who owes you");
        _amount.Placeholder = L.T("৳ মোট টাকা", "৳ Total amount");
        _paid.Placeholder = iOwe ? L.T("৳ এ পর্যন্ত কত শোধ করেছেন", "৳ Paid back so far") : L.T("৳ এ পর্যন্ত কত ফেরত পেয়েছেন", "৳ Received back so far");
        _pay.Placeholder = "৳";
        _note.Placeholder = L.T("নোট (ঐচ্ছিক)", "Note (optional)");
        _due.MinimumDate = new DateTime(2000, 1, 1);
        _due.MaximumDate = new DateTime(2100, 12, 31);
        _due.Format = "dd MMM yyyy";

        _person.Text = _loan.Person;
        if (!isNew)
        {
            _amount.Text = _loan.Amount.ToString("0.##", Fmt.Inv);
            _paid.Text = _loan.Paid > 0 ? _loan.Paid.ToString("0.##", Fmt.Inv) : "";
        }
        _hasDue.IsToggled = _loan.HasDue;
        _due.Date = _loan.DueDate < _due.MinimumDate ? DateTime.Today : _loan.DueDate;
        _note.Text = _loan.Note;
        _due.IsVisible = _loan.HasDue;
        _hasDue.Toggled += (_, e) => _due.IsVisible = e.Value;
        _amount.TextChanged += (_, _) => UpdateRem();
        _paid.TextChanged += (_, _) => UpdateRem();

        ToolbarItems.Add(new ToolbarItem { Text = L.T("বন্ধ করুন", "Close"), Command = new Command(async () => await Navigation.PopAsync()) });

        var main = iOwe ? Ui.Red : Ui.Green;
        var s = new VerticalStackLayout { Padding = new Thickness(16, 14, 16, 30), Spacing = 8 };

        s.Add(FUi.Muted(iOwe ? L.T("কার কাছ থেকে নিয়েছেন", "Borrowed from") : L.T("কাকে দিয়েছেন", "Lent to")));
        s.Add(FUi.Field(_person));
        s.Add(new Label { Text = L.T("মোট টাকা", "Total amount"), FontSize = 13, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        s.Add(FUi.Field(_amount));
        s.Add(new Label { Text = iOwe ? L.T("এ পর্যন্ত শোধ করা", "Paid back so far") : L.T("এ পর্যন্ত ফেরত পাওয়া", "Received back so far"), FontSize = 13, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        s.Add(FUi.Field(_paid));
        s.Add(_remLbl);

        // Quick payment add
        if (!isNew)
        {
            var payRow = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            payRow.Add(FUi.Field(_pay), 0);
            payRow.Add(FUi.Btn(iOwe ? L.T("শোধ যোগ", "Add payment") : L.T("পাওয়া যোগ", "Add received"), main, true, AddPay, 46), 1);
            var pc = new VerticalStackLayout { Spacing = 6 };
            pc.Add(new Label { Text = iOwe ? L.T("আজ আরও কত শোধ করলেন?", "Paid more today?") : L.T("আজ আরও কত ফেরত পেলেন?", "Received more today?"), FontSize = 13, TextColor = Ui.Muted });
            pc.Add(payRow);
            s.Add(FUi.Card(pc, main.WithAlpha(0.4f)));
        }

        var dueRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Margin = new Thickness(0, 8, 0, 0) };
        dueRow.Add(new Label { Text = iOwe ? L.T("কবে দিবেন (তারিখ দেওয়া)", "Set a pay-by date") : L.T("কবে পাবেন (তারিখ দেওয়া)", "Set an expected date"), FontSize = 15, VerticalOptions = LayoutOptions.Center }, 0);
        dueRow.Add(_hasDue, 1);
        s.Add(dueRow);
        s.Add(FUi.Field(_due));
        s.Add(FUi.Muted(L.T("তারিখ দিলে সেটিংসে রিমাইন্ডার চালু থাকলে নোটিফিকেশন আসবে", "If reminders are on in Settings, you'll get a notification for this date"), 12));

        s.Add(new Label { Text = L.T("নোট", "Note"), FontSize = 13, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        s.Add(FUi.Field(_note));

        s.Add(new Button
        {
            Text = L.T("✔ সেভ করুন", "✔ Save"),
            HeightRequest = 54,
            Margin = new Thickness(0, 10, 0, 0),
            BackgroundColor = main,
            Command = new Command(async () => await SaveAsync())
        });
        if (!isNew)
        {
            var del = new Button
            {
                Text = L.T("🗑 মুছুন", "🗑 Delete"),
                HeightRequest = 50,
                BackgroundColor = Ui.Surface,
                TextColor = Ui.Red,
                BorderColor = Ui.Red,
                BorderWidth = 1.5
            };
            del.Clicked += async (_, _) => await DeleteAsync();
            s.Add(del);
        }
        Content = new ScrollView { Content = s };
        UpdateRem();
    }

    void UpdateRem()
    {
        Fmt.TryAmount(_amount.Text, out var a);
        Fmt.TryAmount(_paid.Text, out var p);
        var rem = a - p;
        if (a <= 0) { _remLbl.Text = ""; return; }
        if (rem > 0)
        {
            _remLbl.Text = L.T("বাকি: ", "Remaining: ") + FUi.Money(rem);
            _remLbl.TextColor = _iOwe ? Ui.Red : Ui.Green;
        }
        else
        {
            _remLbl.Text = L.T("✔ পুরোটা শেষ", "✔ Fully settled");
            _remLbl.TextColor = Ui.Muted;
        }
    }

    void AddPay()
    {
        if (!Fmt.TryAmount(_pay.Text, out var x) || x <= 0)
        {
            Ui.Toast(L.T("টাকার অঙ্ক লিখুন", "Enter an amount"));
            return;
        }
        Fmt.TryAmount(_paid.Text, out var p);
        _paid.Text = (p + x).ToString("0.##", Fmt.Inv);
        _pay.Text = "";
        Ui.Toast(L.T("যোগ হয়েছে — সেভ করুন চাপুন", "Added — tap Save to keep it"));
    }

    async Task SaveAsync()
    {
        try
        {
            var name = (_person.Text ?? "").Trim();
            if (name.Length == 0)
            {
                await DisplayAlert(L.T("সমস্যা", "Problem"), L.T("নাম লিখুন", "Enter a name"), L.T("ঠিক আছে", "OK"));
                return;
            }
            if (!Fmt.TryAmount(_amount.Text, out var amt) || amt <= 0)
            {
                await DisplayAlert(L.T("সমস্যা", "Problem"), L.T("সঠিক টাকার অঙ্ক লিখুন", "Enter a valid amount"), L.T("ঠিক আছে", "OK"));
                return;
            }
            decimal paid = 0;
            if (!string.IsNullOrWhiteSpace(_paid.Text) && !Fmt.TryAmount(_paid.Text, out paid))
            {
                await DisplayAlert(L.T("সমস্যা", "Problem"), L.T("শোধের অঙ্ক ঠিক নয়", "Invalid paid amount"), L.T("ঠিক আছে", "OK"));
                return;
            }
            if (paid > amt) paid = amt;

            _loan.Person = name;
            _loan.Amount = amt;
            _loan.Paid = paid;
            _loan.HasDue = _hasDue.IsToggled;
            _loan.DueDate = _due.Date.Date;
            _loan.Note = (_note.Text ?? "").Trim();
            _loan.IOwe = _iOwe;
            await Store.SaveLoanAsync(_loan);
            Ui.Toast(L.T("✔ সেভ হয়েছে", "✔ Saved"));
            await Navigation.PopAsync();
        }
        catch { }
    }

    async Task DeleteAsync()
    {
        try
        {
            if (!await DisplayAlert(L.T("মুছবেন?", "Delete?"), _loan.Person, L.T("মুছুন", "Delete"), L.T("না", "No"))) return;
            await Store.DeleteLoanAsync(_loan);
            await Navigation.PopAsync();
        }
        catch { }
    }
}

// Hint
// Salary: mode 0
// Hint
public class SalaryPage : ContentPage
{
    readonly int _mode;
    DateTime _month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    readonly VerticalStackLayout _body = new() { Spacing = 10 };
    readonly Label _monthLbl = new() { FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
    readonly Button _next = new() { Text = "▶", BackgroundColor = Colors.Transparent, TextColor = Colors.White, FontSize = 18, WidthRequest = 52, HeightRequest = 44, Padding = new Thickness(0) };
    bool _shown;

    public SalaryPage(int mode)
    {
        _mode = mode;
        Title = mode == 0 ? L.T("বেতন / আয়", "Salary / income") : L.T("খরচের পর বাকি", "Left after spending");
        ToolbarItems.Add(new ToolbarItem { Text = L.T("বন্ধ করুন", "Close"), Command = new Command(async () => await CloseAsync()) });

        var prev = new Button { Text = "◀", BackgroundColor = Colors.Transparent, TextColor = Colors.White, FontSize = 18, WidthRequest = 52, HeightRequest = 44, Padding = new Thickness(0) };
        prev.Clicked += async (_, _) => { _month = _month.AddMonths(-1); await LoadAsync(); };
        _next.Clicked += async (_, _) =>
        {
            if (_month < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)) { _month = _month.AddMonths(1); await LoadAsync(); }
        };
        var nav = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        nav.Add(prev, 0);
        nav.Add(_monthLbl, 1);
        nav.Add(_next, 2);
        var navCard = new Border
        {
            Content = nav,
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Ui.PrimaryBright, 0f), new GradientStop(Ui.PrimaryDark, 1f) }, new Point(0, 0), new Point(1, 1))
        };

        var root = new VerticalStackLayout { Padding = new Thickness(16, 14, 16, 30), Spacing = 12 };
        root.Add(navCard);
        root.Add(_body);
        Content = new ScrollView { Content = root };
    }

    void OnChanged() { if (_shown) _ = LoadAsync(); }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _shown = true;
        Store.Changed -= OnChanged;
        Store.Changed += OnChanged;
        await LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _shown = false;
        Store.Changed -= OnChanged;
    }

    protected override bool OnBackButtonPressed()
    {
        if (Navigation.NavigationStack.Count > 1) return base.OnBackButtonPressed();
        _ = CloseAsync();
        return true;
    }

    async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    void OpenEdit(Salary? s)
    {
        try { _ = Navigation.PushAsync(new SalaryEditPage(s, _month)); } catch { }
    }

    async Task LoadAsync()
    {
        try
        {
            _monthLbl.Text = Fmt.MonthTitle(_month);
            var isCur = _month >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _next.IsEnabled = !isCur;
            _next.Opacity = isCur ? 0.3 : 1;

            var end = _month.AddMonths(1);
            var sal = await Store.GetSalariesAsync(_month, end);
            var exp = await Store.GetRangeAsync(_month, end);
            var got = sal.Sum(x => x.Amount);
            var spent = exp.Sum(x => x.Amount);
            var left = got - spent;

            _body.Children.Clear();

            // summary card
            var sum = new VerticalStackLayout { Spacing = 8 };
            sum.Add(SumRow(L.T("💼 বেতন পেয়েছেন", "💼 Salary received"), FUi.Money(got), Ui.Green));
            sum.Add(SumRow(L.T("🧾 খরচ করেছেন", "🧾 Spent"), FUi.Money(spent), Ui.Red));
            sum.Add(new BoxView { HeightRequest = 1, Color = Ui.Line });
            sum.Add(SumRow(left >= 0 ? L.T("✅ খরচের পর বাকি", "✅ Left after spending") : L.T("⚠️ বেতনের চেয়ে বেশি খরচ", "⚠️ Spent more than salary"),
                FUi.Money(left >= 0 ? left : -left), left >= 0 ? Ui.Green : Ui.Red, true));
            if (got > 0)
            {
                double frac = (double)(spent / got);
                sum.Add(Ui.Bar(frac, Ui.LimitColor(frac), null, 12));
                sum.Add(FUi.Muted(Math.Round(frac * 100).ToString(Fmt.Inv) + L.T("% খরচ হয়েছে", "% of salary spent"), 12));
            }
            else
            {
                sum.Add(FUi.Muted(L.T("এই মাসে বেতন যোগ করা হয়নি — নিচে থেকে যোগ করুন", "No salary added for this month — add it below"), 12));
            }
            _body.Add(FUi.Card(sum, null, 16));

            if (_mode == 0) BuildEntries(sal);
            else BuildBreakdown(exp, spent, got);
        }
        catch { }
    }

    static View SumRow(string label, string value, Color color, bool big = false)
    {
        var g = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        g.Add(new Label { Text = label, FontSize = big ? 16 : 15, FontAttributes = big ? FontAttributes.Bold : FontAttributes.None, VerticalOptions = LayoutOptions.Center }, 0);
        g.Add(new Label { Text = value, FontSize = big ? 24 : 18, FontAttributes = FontAttributes.Bold, TextColor = color, VerticalOptions = LayoutOptions.Center }, 1);
        return g;
    }

    void BuildEntries(List<Salary> sal)
    {
        _body.Add(FUi.Btn(L.T("+ বেতন / আয় যোগ করুন", "+ Add salary / income"), Ui.Primary, true, () => OpenEdit(null), 52));
        _body.Add(new Label { Text = L.T("এই মাসে পাওয়া", "Received this month"), FontSize = 18, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 6, 0, 0) });
        if (sal.Count == 0)
        {
            _body.Add(new Label { Text = L.T("এই মাসে কোনো বেতন যোগ করা নেই", "No salary added this month"), TextColor = Ui.Muted });
            return;
        }
        _body.Add(FUi.Muted(L.T("এন্ট্রিতে ট্যাপ করে এডিট বা মুছুন — বেতন বাড়লে/কমলে বদলে নিন", "Tap an entry to edit or delete — update it whenever it changes")));
        foreach (var s in sal)
        {
            var mid = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
            mid.Add(new Label { Text = string.IsNullOrWhiteSpace(s.Note) ? L.T("বেতন", "Salary") : s.Note, FontSize = 16, FontAttributes = FontAttributes.Bold });
            mid.Add(FUi.Muted(s.Date.ToString("dd MMM yyyy", Fmt.Inv), 12));
            var g = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            g.Add(Ui.Badge(Ui.Green, "💼", 42), 0);
            g.Add(mid, 1);
            g.Add(new Label { Text = FUi.Money(s.Amount), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ui.Green, VerticalOptions = LayoutOptions.Center }, 2);
            var card = FUi.Card(g);
            var item = s;
            card.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => OpenEdit(item)) });
            _body.Add(card);
        }
    }

    void BuildBreakdown(List<Expense> exp, decimal spent, decimal got)
    {
        _body.Add(new Label { Text = L.T("কোন খাতে কত খরচ হলো", "Where the money went"), FontSize = 18, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 6, 0, 0) });
        if (exp.Count == 0 || spent <= 0)
        {
            _body.Add(new Label { Text = L.T("এই মাসে কোনো খরচ নেই", "No expenses this month"), TextColor = Ui.Muted });
            return;
        }
        var box = new VerticalStackLayout { Spacing = 12 };
        foreach (var grp in exp.GroupBy(x => x.Cat).Select(g => new { Name = g.Key, Sum = g.Sum(x => x.Amount) }).OrderByDescending(x => x.Sum))
        {
            var color = Ui.ItemColor(grp.Name);
            double share = (double)(grp.Sum / spent);
            var top = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            top.Add(Ui.ItemBadge(grp.Name, 34), 0);
            var nm = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
            nm.Add(new Label { Text = grp.Name, FontSize = 16, FontAttributes = FontAttributes.Bold });
            nm.Add(FUi.Muted(Math.Round(share * 100).ToString(Fmt.Inv) + L.T("% খরচের", "% of spending") + (got > 0 ? " · " + Math.Round((double)(grp.Sum / got) * 100).ToString(Fmt.Inv) + L.T("% বেতনের", "% of salary") : ""), 12));
            top.Add(nm, 1);
            top.Add(new Label { Text = FUi.Money(grp.Sum), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = color, VerticalOptions = LayoutOptions.Center }, 2);
            var v = new VerticalStackLayout { Spacing = 6 };
            v.Add(top);
            v.Add(Ui.Bar(share, color, null, 8));
            box.Add(v);
        }
        _body.Add(FUi.Card(box, null, 16));
    }
}

// Hint
// Salary add /
// Hint
public class SalaryEditPage : ContentPage
{
    readonly Salary _s;
    readonly Entry _amount = new() { Keyboard = Keyboard.Numeric, FontSize = 22, FontAttributes = FontAttributes.Bold };
    readonly Entry _note = new() { MaxLength = 60 };
    readonly DatePicker _date = new() { Format = "dd MMM yyyy" };

    public SalaryEditPage(Salary? existing, DateTime month)
    {
        bool isNew = existing == null;
        var today = DateTime.Today;
        var defDate = (month.Year == today.Year && month.Month == today.Month) ? today : month;
        _s = existing ?? new Salary { Date = defDate };
        Title = isNew ? L.T("বেতন / আয় যোগ", "Add salary / income") : L.T("বেতন এডিট", "Edit salary");

        _amount.Placeholder = L.T("৳ কত টাকা পেয়েছেন", "৳ Amount received");
        _note.Placeholder = L.T("কোথা থেকে / কী বাবদ (ঐচ্ছিক)", "Source / purpose (optional)");
        _date.MinimumDate = new DateTime(2000, 1, 1);
        _date.MaximumDate = new DateTime(2100, 12, 31);
        _date.Date = _s.Date.Date;
        if (!isNew) _amount.Text = _s.Amount.ToString("0.##", Fmt.Inv);
        _note.Text = _s.Note;

        ToolbarItems.Add(new ToolbarItem { Text = L.T("বন্ধ করুন", "Close"), Command = new Command(async () => await Navigation.PopAsync()) });

        var v = new VerticalStackLayout { Padding = new Thickness(16, 14, 16, 30), Spacing = 8 };
        v.Add(FUi.Muted(L.T("টাকার পরিমাণ", "Amount")));
        v.Add(FUi.Field(_amount));
        v.Add(new Label { Text = L.T("কী বাবদ", "For"), FontSize = 13, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        v.Add(FUi.Field(_note));
        v.Add(new Label { Text = L.T("কবে পেয়েছেন", "Date received"), FontSize = 13, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        v.Add(FUi.Field(_date));
        v.Add(new Button { Text = L.T("✔ সেভ করুন", "✔ Save"), HeightRequest = 54, Margin = new Thickness(0, 10, 0, 0), Command = new Command(async () => await SaveAsync()) });
        if (!isNew)
        {
            var del = new Button { Text = L.T("🗑 মুছুন", "🗑 Delete"), HeightRequest = 50, BackgroundColor = Ui.Surface, TextColor = Ui.Red, BorderColor = Ui.Red, BorderWidth = 1.5 };
            del.Clicked += async (_, _) => await DeleteAsync();
            v.Add(del);
        }
        Content = new ScrollView { Content = v };
    }

    async Task SaveAsync()
    {
        try
        {
            if (!Fmt.TryAmount(_amount.Text, out var amt) || amt <= 0)
            {
                await DisplayAlert(L.T("সমস্যা", "Problem"), L.T("সঠিক টাকার অঙ্ক লিখুন", "Enter a valid amount"), L.T("ঠিক আছে", "OK"));
                return;
            }
            _s.Amount = amt;
            _s.Note = (_note.Text ?? "").Trim();
            _s.Date = _date.Date.Date;
            await Store.SaveSalaryAsync(_s);
            Ui.Toast(L.T("✔ সেভ হয়েছে", "✔ Saved"));
            await Navigation.PopAsync();
        }
        catch { }
    }

    async Task DeleteAsync()
    {
        try
        {
            if (!await DisplayAlert(L.T("মুছবেন?", "Delete?"), FUi.Money(_s.Amount), L.T("মুছুন", "Delete"), L.T("না", "No"))) return;
            await Store.DeleteSalaryAsync(_s);
            await Navigation.PopAsync();
        }
        catch { }
    }
}
