using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public abstract class ModalBase : ContentPage
{
    protected ModalBase(string title)
    {
        Title = title;
        ToolbarItems.Add(new ToolbarItem { Text = L.T("বন্ধ করুন", "Close"), Command = new Command(async () => await CloseAsync()) });
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync();
        return true;
    }

    protected async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    protected static VerticalStackLayout NewRoot() => new() { Padding = new Thickness(16, 16, 16, 30), Spacing = 12 };
}

public class LanguagePage : ModalBase
{
    public LanguagePage() : base(L.T("ভাষা", "Language"))
    {
        var root = NewRoot();
        root.Add(FUi.Muted(L.T("অ্যাপ কোন ভাষায় দেখবেন বাছুন", "Choose the language of the app"), 14));
        root.Add(FUi.Option("🇧🇩", "বাংলা", "Bangla", !L.IsEn, () => Pick("bn")));
        root.Add(FUi.Option("🇬🇧", "English", "ইংরেজি", L.IsEn, () => Pick("en")));
        Content = new ScrollView { Content = root };
    }

    async Task Pick(string code)
    {
        if ((code == "en") == L.IsEn) { await CloseAsync(); return; }
        await LangSwitch.ApplyAsync(code, goToSettings: true);
    }
}

public class ThemePage : ModalBase
{
    public ThemePage() : base(L.T("থিম", "Theme"))
    {
        var root = NewRoot();
        root.Add(FUi.Muted(L.T("লাইট বা ডার্ক — যেটা ভালো লাগে বেছে নিন। \"ফোনের মতো\" বাছলে ফোনের থিম অনুযায়ী নিজে বদলাবে।",
            "Pick light or dark. \"Auto\" follows your phone's theme."), 14));
        root.Add(FUi.Option("☀️", L.T("লাইট", "Light"), null, Theme.Mode == Theme.Light, () => Pick(Theme.Light)));
        root.Add(FUi.Option("🌙", L.T("ডার্ক", "Dark"), null, Theme.Mode == Theme.Dark, () => Pick(Theme.Dark)));
        root.Add(FUi.Option("📱", L.T("ফোনের মতো", "Auto"), L.T("ফোনের থিম অনুসরণ করবে", "Follows the phone"), Theme.Mode == Theme.System, () => Pick(Theme.System)));
        Content = new ScrollView { Content = root };
    }

    async Task Pick(string mode)
    {
        if (Theme.Mode == mode) { await CloseAsync(); return; }
        await Theme.SetAsync(mode, goToSettings: true);
    }
}

public class LimitsPage : ModalBase
{
    readonly Entry _daily = new() { Keyboard = Keyboard.Numeric };
    readonly Entry _monthly = new() { Keyboard = Keyboard.Numeric };
    readonly Label _hint = new() { FontSize = 13, TextColor = Ui.Muted };

    public LimitsPage() : base(L.T("লিমিট", "Limits"))
    {
        _daily.Placeholder = L.T("দৈনিক লিমিট (৳)", "Daily limit (৳)");
        _monthly.Placeholder = L.T("মাসিক লিমিট (৳)", "Monthly limit (৳)");
        _daily.Text = AppSettings.DailyLimit > 0 ? AppSettings.DailyLimit.ToString("0.##", Fmt.Inv) : "";
        _monthly.Text = AppSettings.MonthlyLimit > 0 ? AppSettings.MonthlyLimit.ToString("0.##", Fmt.Inv) : "";
        _monthly.TextChanged += (_, _) => UpdateHint();

        var root = NewRoot();
        root.Add(FUi.Muted(L.T("প্রতিদিন সর্বোচ্চ কত টাকা খরচ করবেন", "Most you want to spend per day"), 14));
        root.Add(FUi.Field(_daily));
        root.Add(new Label { Text = L.T("মাসে সর্বোচ্চ কত টাকা খরচ করবেন", "Most you want to spend per month"), FontSize = 14, TextColor = Ui.Muted, Margin = new Thickness(0, 6, 0, 0) });
        root.Add(FUi.Field(_monthly));
        root.Add(_hint);
        root.Add(new Label
        {
            Text = L.T("ℹ️ নিজের তারিখের রেঞ্জের লিমিট এখানে নয় — হিসাব বা রিপোর্ট ট্যাবে (অথবা হোমের 'রেঞ্জ ঠিক করুন' বাটনে) রেঞ্জ ঠিক করার সময়ই লিমিট দিতে হয়।"),
            FontSize = 12,
            TextColor = Ui.Muted
        });
        var save = new Button { Text = L.T("লিমিট সেভ করুন", "Save limits"), HeightRequest = 50, Margin = new Thickness(0, 6, 0, 0) };
        save.Clicked += async (_, _) => await SaveAsync();
        root.Add(save);
        Content = new ScrollView { Content = root };
        UpdateHint();
    }

    void UpdateHint()
    {
        if (Fmt.TryAmount(_monthly.Text, out var m) && m > 0)
            _hint.Text = L.T("মাসিক লিমিট ভাগ করলে দিনে গড়ে ") + Fmt.Money0(m / 30) + L.T(" (দৈনিক লিমিট খালি রাখলে এটাই দৈনিক লিমিট ধরা হবে)");
        else
            _hint.Text = L.T("খালি রাখলে বা ০ দিলে লিমিট বন্ধ থাকবে");
    }

    async Task SaveAsync()
    {
        try
        {
            decimal d = 0, m = 0;
            if (!string.IsNullOrWhiteSpace(_daily.Text) && !Fmt.TryAmount(_daily.Text, out d))
            {
                await DisplayAlert(L.T("সমস্যা"), L.T("দৈনিক লিমিটের অঙ্ক ঠিক নয়"), L.T("ঠিক আছে"));
                return;
            }
            if (!string.IsNullOrWhiteSpace(_monthly.Text) && !Fmt.TryAmount(_monthly.Text, out m))
            {
                await DisplayAlert(L.T("সমস্যা"), L.T("মাসিক লিমিটের অঙ্ক ঠিক নয়"), L.T("ঠিক আছে"));
                return;
            }
            AppSettings.DailyLimit = d;
            AppSettings.MonthlyLimit = m;
            Store.RaiseChanged();
            Ui.Toast(L.T("✔ লিমিট সেভ হয়েছে"));
            await CloseAsync();
        }
        catch (Exception ex) { AppLog.Error("Limits.Save", ex); }
    }
}

public class NotificationsPage : ModalBase
{
    bool _busy;

    public NotificationsPage() : base(L.T("নোটিফিকেশন", "Notifications"))
    {
        _busy = true;
        var root = NewRoot();

        if (!Reminders.NotificationsAllowed())
            root.Add(new Label { Text = L.T("⚠️ ফোনের সেটিংসে এই অ্যাপের নোটিফিকেশন বন্ধ আছে। নোটিফিকেশন পেতে সেটা চালু করুন।", "⚠️ Notifications are turned off for this app in phone settings. Turn them on to get reminders."), TextColor = Ui.Red, FontSize = 13 });

        var dailySwitch = new Switch { IsToggled = Reminders.DailyOn, VerticalOptions = LayoutOptions.Center };
        var dailyTime = new TimePicker { Time = TimeSpan.FromMinutes(Reminders.DailyMinutes), Format = "hh:mm tt", TextColor = Ui.Ink };
        var dTop = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var dTitle = new VerticalStackLayout { Spacing = 0 };
        dTitle.Add(new Label { Text = L.T("প্রতিদিন খরচ লেখার রিমাইন্ডার", "Daily reminder to add expenses"), FontSize = 15, FontAttributes = FontAttributes.Bold });
        dTitle.Add(new Label { Text = L.T("রোজ নির্দিষ্ট সময়ে নোটিফিকেশন আসবে", "You'll get a notification at your chosen time every day"), FontSize = 12, TextColor = Ui.Muted });
        dTop.Add(dTitle, 0);
        dTop.Add(dailySwitch, 1);
        var dRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, IsVisible = Reminders.DailyOn };
        dRow.Add(new Label { Text = L.T("সময়:", "Time:"), VerticalOptions = LayoutOptions.Center }, 0);
        dRow.Add(dailyTime, 1);
        var dBox = new VerticalStackLayout { Spacing = 6 };
        dBox.Add(dTop);
        dBox.Add(dRow);
        dailySwitch.Toggled += async (_, e) =>
        {
            if (_busy) return;
            Reminders.DailyOn = e.Value;
            dRow.IsVisible = e.Value;
            if (e.Value) Reminders.AskPermission();
            await Reminders.RescheduleAsync();
            if (e.Value) Ui.Toast(L.T("✔ প্রতিদিনের রিমাইন্ডার চালু", "✔ Daily reminder on"));
        };
        dailyTime.PropertyChanged += async (_, e) =>
        {
            if (_busy || e.PropertyName != nameof(TimePicker.Time)) return;
            Reminders.DailyMinutes = (int)dailyTime.Time.TotalMinutes;
            await Reminders.RescheduleAsync();
        };
        root.Add(FUi.Card(dBox, null, 14));

        var loanSwitch = new Switch { IsToggled = Reminders.LoanOn, VerticalOptions = LayoutOptions.Center };
        var loanTime = new TimePicker { Time = TimeSpan.FromMinutes(Reminders.LoanMinutes), Format = "hh:mm tt", TextColor = Ui.Ink };
        var days = new Picker
        {
            Title = L.T("কত দিন আগে", "How many days before"),
            ItemsSource = new List<string>
            {
                L.T("ওই দিনেই", "On the day"),
                L.T("১ দিন আগে", "1 day before"),
                L.T("২ দিন আগে", "2 days before"),
                L.T("৩ দিন আগে", "3 days before"),
                L.T("৭ দিন আগে", "7 days before")
            },
            TextColor = Ui.Ink
        };
        int[] dayVals = { 0, 1, 2, 3, 7 };
        days.SelectedIndex = Math.Max(0, Array.IndexOf(dayVals, Reminders.LoanDaysBefore));

        var lTop = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var lTitle = new VerticalStackLayout { Spacing = 0 };
        lTitle.Add(new Label { Text = L.T("ধার-দেনার রিমাইন্ডার", "Loan & debt reminders"), FontSize = 15, FontAttributes = FontAttributes.Bold });
        lTitle.Add(new Label { Text = L.T("কাকে কবে দিতে হবে / কার থেকে কবে পাবেন তা মনে করিয়ে দেবে", "Reminds you whom to pay and from whom to collect, and when"), FontSize = 12, TextColor = Ui.Muted });
        lTop.Add(lTitle, 0);
        lTop.Add(loanSwitch, 1);
        var lRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 8, IsVisible = Reminders.LoanOn };
        lRow.Add(new Label { Text = L.T("সময়:", "Time:"), VerticalOptions = LayoutOptions.Center }, 0);
        lRow.Add(loanTime, 1);
        lRow.Add(days, 2);
        var lBox = new VerticalStackLayout { Spacing = 6 };
        lBox.Add(lTop);
        lBox.Add(lRow);
        loanSwitch.Toggled += async (_, e) =>
        {
            if (_busy) return;
            Reminders.LoanOn = e.Value;
            lRow.IsVisible = e.Value;
            if (e.Value) Reminders.AskPermission();
            await Reminders.RescheduleAsync();
            if (e.Value) Ui.Toast(L.T("✔ ধার-দেনার রিমাইন্ডার চালু", "✔ Loan reminders on"));
        };
        loanTime.PropertyChanged += async (_, e) =>
        {
            if (_busy || e.PropertyName != nameof(TimePicker.Time)) return;
            Reminders.LoanMinutes = (int)loanTime.Time.TotalMinutes;
            await Reminders.RescheduleAsync();
        };
        days.SelectedIndexChanged += async (_, _) =>
        {
            if (_busy || days.SelectedIndex < 0) return;
            Reminders.LoanDaysBefore = dayVals[days.SelectedIndex];
            await Reminders.RescheduleAsync();
        };
        root.Add(FUi.Card(lBox, null, 14));

        var updSwitch = new Switch { IsToggled = UpdateChecker.On, VerticalOptions = LayoutOptions.Center };
        var uTop = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var uTitle = new VerticalStackLayout { Spacing = 0 };
        uTitle.Add(new Label { Text = L.T("অ্যাপ আপডেটের নোটিফিকেশন", "App update notifications"), FontSize = 15, FontAttributes = FontAttributes.Bold });
        uTitle.Add(new Label { Text = L.T("প্রতিদিন একবার নতুন ভার্সন এসেছে কি না দেখবে (ইন্টারনেট লাগবে); এলে জানাবে", "Checks once a day for a new version (needs internet) and lets you know"), FontSize = 12, TextColor = Ui.Muted });
        uTop.Add(uTitle, 0);
        uTop.Add(updSwitch, 1);
        updSwitch.Toggled += (_, e) =>
        {
            if (_busy) return;
            UpdateChecker.On = e.Value;
            var ctx = global::Android.App.Application.Context;
            if (e.Value) { Reminders.AskPermission(); UpdateChecker.Schedule(ctx); }
            else UpdateChecker.Cancel(ctx);
        };
        root.Add(FUi.Card(uTop, null, 14));

        var test = new Button { Text = L.T("🔔 টেস্ট নোটিফিকেশন পাঠান", "🔔 Send a test notification"), StyleClass = new[] { "Outline" }, HeightRequest = 44, FontSize = 14 };
        test.Clicked += (_, _) =>
        {
            Reminders.AskPermission();
            Reminders.ShowTest();
        };
        root.Add(test);

        Content = new ScrollView { Content = root };
        _busy = false;
    }
}

public class CategoriesPage : ModalBase
{
    readonly VerticalStackLayout _stack = new() { Spacing = 12 };
    bool _shown;

    public CategoriesPage() : base(L.T("ক্যাটাগরি ও অপশন", "Categories & options"))
    {
        var root = NewRoot();
        root.Add(new Label
        {
            Text = L.T("খরচ যোগ করার সময় আগে ক্যাটাগরি বাছবেন। 'অপশন' ক্যাটাগরিতে ভিতর থেকে বেছে নেবেন (যেমন খাওয়া → দুপুর); 'লেখা' ক্যাটাগরিতে কী কিনলেন নিজে লিখবেন (যেমন কেনাকাটা)। নাম বদলালে আগের হিসাবেও বদলায়; মুছলে আগের হিসাব মুছবে না।"),
            FontSize = 13,
            TextColor = Ui.Muted
        });
        root.Add(_stack);
        var add = new Button { Text = L.T("+ নতুন ক্যাটাগরি যোগ করুন", "+ Add a new category"), StyleClass = new[] { "Outline" }, HeightRequest = 50 };
        add.Clicked += async (_, _) => await AddCategoryAsync();
        root.Add(add);
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

    async Task LoadAsync()
    {
        try
        {
            var cats = await Store.GetCategoriesAsync();
            var choices = await Store.GetChoicesAsync();
            _stack.Children.Clear();
            foreach (var cat in cats)
                _stack.Add(BuildCategoryCard(cat, choices.Where(c => c.CategoryId == cat.Id).ToList()));
        }
        catch (Exception ex) { AppLog.Error("Cats.Load", ex); }
    }

    View BuildCategoryCard(ExpenseCategory cat, List<Choice> options)
    {
        var color = Ui.ItemColor(cat.Name);

        var title = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        title.Add(new Label { Text = cat.Name, FontSize = 18, FontAttributes = FontAttributes.Bold });
        title.Add(new Label
        {
            Text = cat.IsFreeText ? L.T("লেখা ক্যাটাগরি (কী কিনলেন লিখতে হয়)") : L.T("অপশন ক্যাটাগরি · ") + options.Count.ToString(Fmt.Inv) + L.T("টি অপশন"),
            FontSize = 12,
            TextColor = color,
            FontAttributes = FontAttributes.Bold
        });

        var head = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        head.Add(Ui.Badge(color, cat.Icon, 44), 0);
        head.Add(title, 1);

        var actions = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        var ren = MiniButton(L.T("✎ নাম বদলান"), color, false);
        ren.Clicked += async (_, _) => await RenameCategoryAsync(cat);
        var del = MiniButton(L.T("🗑 মুছুন"), Ui.Red, false);
        del.Clicked += async (_, _) => await DeleteCategoryAsync(cat);
        actions.Add(ren, 0);
        actions.Add(del, 1);

        var body = new VerticalStackLayout { Spacing = 8 };
        body.Add(head);
        if (!cat.IsFreeText)
        {
            foreach (var c in options) body.Add(BuildChoiceRow(c, color));
            var add = MiniButton("+ " + cat.Name + L.T(" এ নতুন অপশন যোগ করুন"), color, true);
            add.Clicked += async (_, _) => await AddChoiceAsync(cat);
            body.Add(add);
        }
        body.Add(actions);

        return new Border
        {
            Content = body,
            Padding = new Thickness(14, 12),
            BackgroundColor = Ui.Surface,
            Stroke = color.WithAlpha(0.45f),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) }
        };
    }

    static Button MiniButton(string text, Color color, bool filled) => new Button
    {
        Text = text,
        FontSize = 14,
        HeightRequest = 42,
        Padding = new Thickness(10, 0),
        CornerRadius = 21,
        BorderWidth = 1.2,
        BorderColor = color,
        BackgroundColor = filled ? color.WithAlpha(0.12f) : Ui.Surface,
        TextColor = color
    };

    View BuildChoiceRow(Choice c, Color color)
    {
        var rename = new Button { Text = L.T("নাম বদলান"), StyleClass = new[] { "Ghost" }, FontSize = 13, HeightRequest = 38, Padding = new Thickness(8, 0) };
        rename.Clicked += async (_, _) => await RenameAsync(c);
        var del = new Button { Text = L.T("মুছুন"), StyleClass = new[] { "Ghost" }, TextColor = Ui.Red, FontSize = 13, HeightRequest = 38, Padding = new Thickness(8, 0) };
        del.Clicked += async (_, _) => await DeleteAsync(c);

        var g = new Grid
        {
            ColumnSpacing = 0,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }
        };
        g.Add(new Label { Text = Ui.ItemIcon(c.Name) + "  " + c.Name, FontSize = 16, VerticalOptions = LayoutOptions.Center }, 0);
        g.Add(rename, 1);
        g.Add(del, 2);

        return new Border
        {
            Content = g,
            Padding = new Thickness(12, 2, 4, 2),
            BackgroundColor = color.WithAlpha(0.07f),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }
        };
    }

    async Task AddCategoryAsync()
    {
        try
        {
            var kind = await DisplayActionSheet(L.T("কেমন ক্যাটাগরি?"), L.T("বাতিল"), null,
                L.T("অপশন থেকে বাছা (যেমন: খাওয়া, যাতায়াত)"),
                L.T("নিজে লিখে দেওয়া (যেমন: কেনাকাটা, Extra)"));
            if (kind == null || kind == L.T("বাতিল")) return;
            bool free = kind == L.T("নিজে লিখে দেওয়া (যেমন: কেনাকাটা, Extra)");

            var name = await DisplayPromptAsync(L.T("নতুন ক্যাটাগরি"), L.T("ক্যাটাগরির নাম লিখুন"), L.T("যোগ করুন"), L.T("বাতিল"), L.T("নাম"), 30);
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!await Store.AddCategoryAsync(name, free))
                await DisplayAlert(L.T("সমস্যা"), L.T("এই নামে আগে থেকেই ক্যাটাগরি আছে"), L.T("ঠিক আছে"));
        }
        catch (Exception ex) { AppLog.Error("Cats.Add", ex); }
    }

    async Task RenameCategoryAsync(ExpenseCategory cat)
    {
        try
        {
            var name = await DisplayPromptAsync(L.T("ক্যাটাগরির নাম বদলান"), L.T("নতুন নাম লিখুন"), L.T("সেভ"), L.T("বাতিল"), L.T("নাম"), 30, null, cat.Name);
            if (string.IsNullOrWhiteSpace(name) || name.Trim() == cat.Name) return;
            if (!await Store.RenameCategoryAsync(cat, name))
                await DisplayAlert(L.T("সমস্যা"), L.T("এই নামে আগে থেকেই ক্যাটাগরি আছে"), L.T("ঠিক আছে"));
        }
        catch (Exception ex) { AppLog.Error("Cats.Rename", ex); }
    }

    async Task DeleteCategoryAsync(ExpenseCategory cat)
    {
        try
        {
            var ok = await DisplayAlert(L.T("মুছবেন?"), "\"" + cat.Name + L.T("\" ক্যাটাগরি ও তার সব অপশন তালিকা থেকে সরে যাবে। আগের হিসাব মুছবে না।"), L.T("মুছুন"), L.T("না"));
            if (ok) await Store.DeleteCategoryAsync(cat);
        }
        catch (Exception ex) { AppLog.Error("Cats.Delete", ex); }
    }

    async Task AddChoiceAsync(ExpenseCategory cat)
    {
        try
        {
            var name = await DisplayPromptAsync(cat.Name + L.T(" — নতুন অপশন"), L.T("নাম লিখুন"), L.T("যোগ করুন"), L.T("বাতিল"), L.T("নাম"), 40);
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!await Store.AddChoiceAsync(name, cat.Id))
                await DisplayAlert(L.T("সমস্যা"), L.T("এই ক্যাটাগরিতে এই নামে অপশন আগে থেকেই আছে"), L.T("ঠিক আছে"));
        }
        catch (Exception ex) { AppLog.Error("Cats.AddChoice", ex); }
    }

    async Task RenameAsync(Choice c)
    {
        try
        {
            var name = await DisplayPromptAsync(L.T("নাম বদলান"), L.T("নতুন নাম লিখুন"), L.T("সেভ"), L.T("বাতিল"), L.T("নাম"), 40, null, c.Name);
            if (string.IsNullOrWhiteSpace(name) || name.Trim() == c.Name) return;
            if (!await Store.RenameChoiceAsync(c, name))
                await DisplayAlert(L.T("সমস্যা"), L.T("এই নামে আগে থেকেই অপশন আছে"), L.T("ঠিক আছে"));
        }
        catch (Exception ex) { AppLog.Error("Cats.RenameChoice", ex); }
    }

    async Task DeleteAsync(Choice c)
    {
        try
        {
            var ok = await DisplayAlert(L.T("মুছবেন?"), "\"" + c.Name + L.T("\" অপশনটি তালিকা থেকে সরে যাবে। আগের হিসাব মুছবে না।"), L.T("মুছুন"), L.T("না"));
            if (ok) await Store.DeleteChoiceAsync(c);
        }
        catch (Exception ex) { AppLog.Error("Cats.DeleteChoice", ex); }
    }
}

public class BackupPage : ModalBase
{
    readonly VerticalStackLayout _root = NewRoot();

    public BackupPage() : base(L.T("ব্যাকআপ ও রিস্টোর", "Backup & restore"))
    {
        Content = new ScrollView { Content = _root };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await BuildAsync();
    }

    public static async Task<(string Title, string Sub, Color Color)> StatusAsync()
    {
        var last = Backup.LastBackup;
        if (last == null)
            return (L.T("⚠️ এখনো ব্যাকআপ নেওয়া হয়নি", "⚠️ No backup yet"), "", Ui.Red);

        var extra = await Backup.NewSinceLastAsync();
        var title = L.T("✅ শেষ ব্যাকআপ: ", "✅ Last backup: ") + last.Value.ToString("dd MMM yyyy, hh:mm tt", Fmt.Inv);
        var parts = new List<string>();
        if (Backup.LastWhere.Length > 0) parts.Add(Backup.LastWhere);
        if (Backup.LastVerified) parts.Add(L.T("ফাইল যাচাই হয়েছে", "file checked"));
        parts.Add(extra > 0 ? L.T("এরপর ", "") + extra.ToString(Fmt.Inv) + L.T("টি নতুন এন্ট্রি", " new entries since") : L.T("সব ঠিক আছে", "all up to date"));
        var color = extra > 0 ? Ui.Orange : Ui.Green;
        if (Backup.IsDue) color = Ui.Red;
        return (title, string.Join(" · ", parts), color);
    }

    async Task BuildAsync()
    {
        try
        {
            _root.Children.Clear();

            var (title, sub, color) = await StatusAsync();
            var st = new VerticalStackLayout { Spacing = 4 };
            st.Add(new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = color });
            if (sub.Length > 0) st.Add(new Label { Text = sub, FontSize = 13, TextColor = Ui.Muted });
            if (Backup.LastError.Length > 0)
                st.Add(new Label { Text = L.T("⚠️ শেষ চেষ্টা ব্যর্থ: ", "⚠️ Last try failed: ") + Backup.LastError, FontSize = 12, TextColor = Ui.Red });
            if (Backup.NextDue is DateTime nd)
                st.Add(new Label
                {
                    Text = Backup.IsDue ? L.T("⏰ ব্যাকআপের সময় পেরিয়ে গেছে", "⏰ Backup is overdue") : L.T("পরের ব্যাকআপ: ", "Next backup: ") + nd.ToString("dd MMM yyyy", Fmt.Inv),
                    FontSize = 12,
                    TextColor = Backup.IsDue ? Ui.Red : Ui.Muted
                });
            _root.Add(FUi.Card(st, color.WithAlpha(0.5f), 14));

            _root.Add(new Label { Text = L.T("📍 কোথায় রাখবেন", "📍 Where to keep it"), FontSize = 17, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 4, 0, 0) });

            bool linked = Backup.HasLinked;
            _root.Add(FUi.Option("📲", L.T("প্রতিবার জিজ্ঞেস করবে", "Ask every time"),
                L.T("ব্যাকআপ নেওয়ার সময় ড্রাইভ বা ফাইলসে জায়গা বেছে নেবেন", "Choose Drive or Files each time you back up"),
                !linked, async () =>
                {
                    Backup.Mode = Backup.Ask;
                    Backup.TargetUri = "";
                    Backup.TargetLabel = "";
                    await BuildAsync();
                }));

            _root.Add(FUi.Option("📁", L.T("ফোল্ডার লিংক করুন", "Link a folder"),
                linked && Backup.Mode == Backup.Folder ? Backup.TargetLabel : L.T("ফোনের বা SD কার্ডের ফোল্ডার — অটো ব্যাকআপ এখানে যাবে", "A folder on phone or SD card — automatic backups go here"),
                linked && Backup.Mode == Backup.Folder, async () =>
                {
                    var u = await Saf.PickFolderAsync();
                    if (u == null) return;
                    var name = Saf.LabelOf(u, true);
                    var prov = Saf.ProviderOf(u);
                    Backup.Mode = Backup.Folder;
                    Backup.TargetUri = u;
                    Backup.TargetLabel = (prov.Length > 0 ? prov + " · " : "") + (name.Length > 0 ? name : L.T("ফোল্ডার", "Folder"));
                    await BuildAsync();
                }));

            _root.Add(FUi.Option("☁️", L.T("গুগল ড্রাইভে লিংক করুন", "Link Google Drive"),
                linked && Backup.Mode == Backup.OneFile ? Backup.TargetLabel : L.T("ড্রাইভ বা অন্য জায়গায় একটা ফাইল বাছুন — প্রতিবার সেই ফাইলেই নতুন ব্যাকআপ বসবে", "Pick a file on Drive or elsewhere — each backup overwrites that file"),
                linked && Backup.Mode == Backup.OneFile, async () =>
                {
                    var u = await Saf.PickNewFileAsync("PocketNama-Backup.json");
                    if (u == null) return;
                    var name = Saf.LabelOf(u, false);
                    var prov = Saf.ProviderOf(u);
                    Backup.Mode = Backup.OneFile;
                    Backup.TargetUri = u;
                    Backup.TargetLabel = (prov.Length > 0 ? prov + " · " : "") + (name.Length > 0 ? name : "PocketNama-Backup.json");
                    await BuildAsync();
                }));

            _root.Add(new Label { Text = L.T("🗓 কতদিন পর পর", "🗓 How often"), FontSize = 17, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 8, 0, 0) });
            var fg = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 8,
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
            };
            string[] fs = { "off", "daily", "weekly", "monthly" };
            for (int i = 0; i < fs.Length; i++)
            {
                var f = fs[i];
                bool sel = Backup.Freq == f;
                var b = new Button
                {
                    Text = (sel ? "✓ " : "") + Backup.FreqName(f),
                    HeightRequest = 46,
                    CornerRadius = 23,
                    FontSize = 14,
                    BorderWidth = 1.5,
                    BorderColor = Ui.Primary,
                    BackgroundColor = sel ? Ui.Primary : Ui.Surface,
                    TextColor = sel ? Colors.White : Ui.Primary
                };
                b.Clicked += async (_, _) =>
                {
                    Backup.Freq = f;
                    await BuildAsync();
                };
                fg.Add(b, i % 2, i / 2);
            }
            _root.Add(fg);
            _root.Add(new Label
            {
                Text = linked
                    ? L.T("অটো ব্যাকআপ অ্যাপ খুললে সময় হয়েছে কি না দেখে নিজে থেকে হয়ে যাবে।", "Automatic backup runs when you open the app and it is due.")
                    : L.T("সময় হলে অ্যাপ খুললে ব্যাকআপ নিতে বলবে; তখন জায়গা বেছে নেবেন।", "When it is due, the app asks you to back up and you choose the place."),
                FontSize = 12,
                TextColor = Ui.Muted
            });

            var now = new Button
            {
                Text = linked ? L.T("⬆ এখনই ব্যাকআপ নিন", "⬆ Back up now") : L.T("⬆ জায়গা বেছে ব্যাকআপ নিন", "⬆ Choose place & back up"),
                HeightRequest = 52,
                Margin = new Thickness(0, 10, 0, 0)
            };
            now.Clicked += async (_, _) =>
            {
                now.IsEnabled = false;
                var r = await Backup.RunAsync(true);
                Ui.Toast(r.Message);
                await BuildAsync();
            };
            _root.Add(now);

            var restore = new Button { Text = L.T("⬇ ব্যাকআপ থেকে ফিরিয়ে আনুন", "⬇ Restore from a backup"), StyleClass = new[] { "Outline" }, HeightRequest = 50 };
            restore.Clicked += async (_, _) =>
            {
                await RestoreFlowAsync(this);
                await BuildAsync();
            };
            _root.Add(restore);

            var share = new Button { Text = L.T("↗ ফাইল শেয়ার করুন (হোয়াটসঅ্যাপ ইত্যাদি)", "↗ Share a copy (WhatsApp etc.)"), StyleClass = new[] { "Ghost" }, FontSize = 14, HeightRequest = 44 };
            share.Clicked += async (_, _) =>
            {
                try { await Backup.ShareCopyAsync(); }
                catch { Ui.Toast(L.T("ব্যাকআপ তৈরি করা যায়নি", "Could not create the backup")); }
            };
            _root.Add(share);
            _root.Add(new Label
            {
                Text = L.T("শেয়ার করা কপি কোথায় সেভ হলো তা অ্যাপ জানতে পারে না, তাই সেটা ব্যাকআপ হিসেবে গোনা হয় না।", "The app cannot know where a shared copy ends up, so it is not counted as a backup."),
                FontSize = 12,
                TextColor = Ui.Muted
            });
        }
        catch (Exception ex) { AppLog.Error("BackupPage.Build", ex); }
    }

    public static async Task RestoreFlowAsync(Page page)
    {
        try
        {
            var ok = await page.DisplayAlert(L.T("ফিরিয়ে আনবেন?"),
                L.T("বর্তমানের সব খরচ, ক্যাটাগরি, লিমিট ও প্রোফাইল মুছে ব্যাকআপ ফাইলের ডেটা বসবে। আগে বর্তমান ডেটার ব্যাকআপ নিয়ে রাখা ভালো। এগিয়ে যাবেন?"),
                L.T("হ্যাঁ, ফাইল বাছুন"), L.T("না"));
            if (!ok) return;

            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = L.T("ব্যাকআপ ফাইল বাছুন") });
            if (file == null) return;

            var (success, msg) = await Backup.RestoreAsync(file);
            if (success)
            {
                Ui.Toast("✔ " + msg);
                Store.RaiseChanged();
                Profile.RaiseChanged();
            }
            else
            {
                await page.DisplayAlert(L.T("সমস্যা"), msg, L.T("ঠিক আছে"));
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Backup.Restore", ex);
            Ui.Toast(L.T("ফাইল বাছাই করা যায়নি"));
        }
    }
}
