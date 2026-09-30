using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        Store.Changed -= OnStoreChanged;
        Profile.Changed -= OnProfileChanged;
        RootChrome.Attach(this);
    }

    void OnProfileChanged() { if (_appeared) RefreshProfile(); }

    void RefreshProfile()
    {
        try
        {
            AvatarHost.Content = Ui.Avatar(68);
            ProfNameLabel.Text = Profile.Name.Length > 0 ? Profile.Name : L.T("নাম দেওয়া হয়নি");
            ProfSubLabel.Text = Profile.Phone.Length > 0 ? Profile.Phone
                : Profile.Email.Length > 0 ? Profile.Email
                : L.T("ছবি ও তথ্য যোগ করতে এডিট চাপুন");
        }
        catch { }
    }

    void OnViewProfile(object? sender, EventArgs e) => Ui.OpenProfile(this);

    void OnEditProfile(object? sender, EventArgs e) => Ui.OpenProfileEdit(this);

    bool _appeared;

    void OnStoreChanged()
    {
        if (!_appeared) return;
        _ = RefreshFinanceAsync();
        _ = BuildCategoryCardAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appeared = false;
        Store.Changed -= OnStoreChanged;
        Profile.Changed -= OnProfileChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appeared = true;
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;
        Profile.Changed -= OnProfileChanged;
        Profile.Changed += OnProfileChanged;
        RefreshProfile();
        BuildPrefBoxes();
        BuildAboutSection();
        await RefreshFinanceAsync();
        await BuildCategoryCardAsync();
        await BuildBackupCardAsync();
    }

    async Task RefreshFinanceAsync()
    {
        try
        {
            var owe = await Store.LoanTotalAsync(true);
            var get = await Store.LoanTotalAsync(false);

            var today = DateTime.Today;
            var ms = new DateTime(today.Year, today.Month, 1);
            var sal = await Store.GetSalariesAsync(ms, ms.AddMonths(1));
            var exp = await Store.GetRangeAsync(ms, ms.AddMonths(1));
            var got = sal.Sum(x => x.Amount);
            var spent = exp.Sum(x => x.Amount);
            var left = got - spent;

            FinanceHost.Children.Clear();

            var g1 = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            g1.Add(StatCard("📤", L.T("ঋণ (আমি দেব)", "Debts (I owe)"), Fmt.Money0(owe.Total),
                owe.Count.ToString(Fmt.Inv) + L.T(" জনকে", " people"), Ui.Red, () => Ui.OpenModal(this, new LoanPage(true))), 0);
            g1.Add(StatCard("📥", L.T("পাওনা (আমি পাব)", "To receive"), Fmt.Money0(get.Total),
                get.Count.ToString(Fmt.Inv) + L.T(" জনের কাছে", " people"), Ui.Green, () => Ui.OpenModal(this, new LoanPage(false))), 1);
            FinanceHost.Add(FUi.Fieldset(L.T("💰 ধার-দেনা", "💰 Loans & debts"), g1));

            var g2 = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            g2.Add(StatCard("💵", L.T("বেতন পেয়েছি", "Salary received"), Fmt.Money0(got),
                sal.Count.ToString(Fmt.Inv) + L.T("টি এন্ট্রি", " entries"), Ui.Primary, () => Ui.OpenModal(this, new SalaryPage(0))), 0);
            g2.Add(StatCard(left >= 0 ? "✅" : "⚠️", L.T("খরচের পর বাকি", "Left after spending"), Fmt.Money0(left >= 0 ? left : -left),
                left >= 0 ? L.T("খরচ ", "Spent ") + Fmt.Money0(spent) : L.T("বেশি খরচ হয়েছে", "Overspent"), left >= 0 ? Ui.Green : Ui.Red,
                () => Ui.OpenModal(this, new SalaryPage(1))), 1);
            FinanceHost.Add(FUi.Fieldset(L.T("💼 বেতন · ", "💼 Salary · ") + Fmt.MonthTitle(ms), g2));
        }
        catch { }
    }

    static View StatCard(string icon, string title, string amount, string sub, Color color, Action onTap)
    {
        var v = new VerticalStackLayout { Spacing = 2 };
        var top = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 6 };
        top.Add(new Label { Text = icon, FontSize = 18, VerticalOptions = LayoutOptions.Center }, 0);
        top.Add(new Label { Text = title, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Ui.Muted, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation }, 1);
        v.Add(top);
        v.Add(new Label { Text = amount, FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = color, Margin = new Thickness(0, 4, 0, 0) });
        v.Add(new Label { Text = sub, FontSize = 11, TextColor = Ui.Muted });
        v.Add(new Label { Text = L.T("সব দেখুন ›", "View all ›"), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = color, Margin = new Thickness(0, 4, 0, 0) });

        var b = new Border
        {
            Content = v,
            Padding = new Thickness(12, 10),
            BackgroundColor = color.WithAlpha(0.07f),
            Stroke = color.WithAlpha(0.45f),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return b;
    }

    void BuildPrefBoxes()
    {
        PrefHost.Children.Clear();

        int active = (Reminders.DailyOn ? 1 : 0) + (Reminders.LoanOn ? 1 : 0);
        var nBody = new VerticalStackLayout { Spacing = 2 };
        nBody.Add(new Label { Text = active.ToString(Fmt.Inv) + L.T("টি চালু", " active"), FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = active > 0 ? Ui.Green : Ui.Muted });
        nBody.Add(new Label
        {
            Text = active == 0 ? L.T("সব বন্ধ", "All off")
                : (Reminders.DailyOn ? L.T("প্রতিদিন", "Daily") : "") + (Reminders.DailyOn && Reminders.LoanOn ? " · " : "") + (Reminders.LoanOn ? L.T("ধার-দেনা", "Loans") : ""),
            FontSize = 12,
            TextColor = Ui.Muted
        });

        var lBody = new VerticalStackLayout { Spacing = 2 };
        lBody.Add(new Label { Text = L.T("দৈনিক ", "Daily ") + (AppSettings.DailyLimit > 0 ? Fmt.Money0(AppSettings.DailyLimit) : L.T("নেই", "none")), FontSize = 14, FontAttributes = FontAttributes.Bold });
        lBody.Add(new Label { Text = L.T("মাসিক ", "Monthly ") + (AppSettings.MonthlyLimit > 0 ? Fmt.Money0(AppSettings.MonthlyLimit) : L.T("নেই", "none")), FontSize = 14, FontAttributes = FontAttributes.Bold });

        var g1 = TwoCols(
            MiniCard("🔔", L.T("নোটিফিকেশন", "Notifications"), nBody, Ui.Green, () => Ui.OpenModal(this, new NotificationsPage())),
            MiniCard("📏", L.T("লিমিট", "Limits"), lBody, Ui.Orange, () => Ui.OpenModal(this, new LimitsPage())));
        PrefHost.Add(FUi.Box(g1));

        var gBody = new VerticalStackLayout { Spacing = 2 };
        gBody.Add(new Label { Text = L.IsEn ? "English" : "বাংলা", FontSize = 20, FontAttributes = FontAttributes.Bold });

        var tBody = new VerticalStackLayout { Spacing = 2 };
        var tIcon = Theme.Mode == Theme.Dark ? "🌙 " : Theme.Mode == Theme.Light ? "☀️ " : "📱 ";
        tBody.Add(new Label { Text = tIcon + RootChrome.ThemeName(), FontSize = 20, FontAttributes = FontAttributes.Bold });

        var g2 = TwoCols(
            MiniCard("🌐", L.T("ভাষা", "Language"), gBody, Ui.Primary, () => Ui.OpenModal(this, new LanguagePage())),
            MiniCard("🎨", L.T("থিম", "Theme"), tBody, Color.FromArgb("#8B5CF6"), () => Ui.OpenModal(this, new ThemePage())));
        PrefHost.Add(FUi.Box(g2));
    }

    static Grid TwoCols(View a, View b)
    {
        var g = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        g.Add(a, 0);
        g.Add(b, 1);
        return g;
    }

    static View MiniCard(string icon, string title, View body, Color color, Action onTap)
    {
        var v = new VerticalStackLayout { Spacing = 4 };
        var top = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 6 };
        top.Add(new Label { Text = icon, FontSize = 18, VerticalOptions = LayoutOptions.Center }, 0);
        top.Add(new Label { Text = title, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Ui.Muted, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation }, 1);
        v.Add(top);
        v.Add(body);
        v.Add(new Label { Text = L.T("বিস্তারিত ›", "Details ›"), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = color, Margin = new Thickness(0, 2, 0, 0) });

        var b = new Border
        {
            Content = v,
            Padding = new Thickness(12, 10),
            BackgroundColor = color.WithAlpha(0.07f),
            Stroke = color.WithAlpha(0.45f),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return b;
    }

    async Task BuildCategoryCardAsync()
    {
        try
        {
            var cats = await Store.GetCategoriesAsync();
            var choices = await Store.GetChoicesAsync();

            var v = new VerticalStackLayout { Spacing = 10 };
            var head = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            head.Add(new Label { Text = L.T("🗂️ ক্যাটাগরি ও অপশন", "🗂️ Categories & options"), StyleClass = new[] { "H2" }, VerticalOptions = LayoutOptions.Center }, 0);
            head.Add(new Label { Text = L.T("✎ এডিট ›", "✎ Edit ›"), FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Ui.Primary, VerticalOptions = LayoutOptions.Center }, 1);
            v.Add(head);

            foreach (var cat in cats)
            {
                var color = Ui.ItemColor(cat.Name);
                var opts = cat.IsFreeText
                    ? L.T("লেখা ক্যাটাগরি", "Text category")
                    : string.Join(", ", choices.Where(c => c.CategoryId == cat.Id).Select(c => c.Name));
                if (opts.Length == 0) opts = "—";

                var col = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
                col.Add(new Label { Text = cat.Name, FontSize = 15, FontAttributes = FontAttributes.Bold });
                col.Add(new Label { Text = opts, FontSize = 12, TextColor = Ui.Muted, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation });

                var row = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
                row.Add(Ui.Badge(color, cat.Icon, 34), 0);
                row.Add(col, 1);
                v.Add(row);
            }

            var card = FUi.Card(v, Ui.Primary.WithAlpha(0.45f), 14);
            card.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Ui.OpenModal(this, new CategoriesPage())) });
            CatHost.Children.Clear();
            CatHost.Add(card);
        }
        catch (Exception ex) { AppLog.Error("Settings.Cats", ex); }
    }

    async Task BuildBackupCardAsync()
    {
        try
        {
            var (title, sub, color) = await BackupPage.StatusAsync();

            var v = new VerticalStackLayout { Spacing = 8 };
            var head = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            head.Add(new Label { Text = L.T("💾 ব্যাকআপ ও রিস্টোর", "💾 Backup & restore"), StyleClass = new[] { "H2" }, VerticalOptions = LayoutOptions.Center }, 0);
            head.Add(new Label { Text = L.T("সেটিংস ›", "Options ›"), FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Ui.Primary, VerticalOptions = LayoutOptions.Center }, 1);
            v.Add(head);
            v.Add(new Label { Text = title, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = color });
            if (sub.Length > 0) v.Add(new Label { Text = sub, FontSize = 12, TextColor = Ui.Muted });
            v.Add(new Label
            {
                Text = L.T("জায়গা: ", "Place: ") + Backup.TargetSummary() + "  ·  " + Backup.FreqName(Backup.Freq),
                FontSize = 12,
                TextColor = Ui.Muted
            });

            var now = new Button { Text = L.T("⬆ ব্যাকআপ নিন", "⬆ Back up"), HeightRequest = 44, FontSize = 14 };
            now.Clicked += async (_, _) =>
            {
                now.IsEnabled = false;
                var r = await Backup.RunAsync(true);
                Ui.Toast(r.Message);
                await BuildBackupCardAsync();
            };
            var res = new Button { Text = L.T("⬇ ফিরিয়ে আনুন", "⬇ Restore"), StyleClass = new[] { "Outline" }, HeightRequest = 44, FontSize = 14 };
            res.Clicked += async (_, _) =>
            {
                await BackupPage.RestoreFlowAsync(this);
                await BuildBackupCardAsync();
            };
            var btns = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 4, 0, 0), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            btns.Add(now, 0);
            btns.Add(res, 1);
            v.Add(btns);

            var card = FUi.Card(v, color.WithAlpha(0.5f), 14);
            var tap = new TapGestureRecognizer { Command = new Command(() => Ui.OpenModal(this, new BackupPage())) };
            head.GestureRecognizers.Add(tap);

            BackupHost.Children.Clear();
            BackupHost.Add(card);
            BackupNote.Text = L.T(
                "ব্যাকআপ ফোনের ফোল্ডারে, গুগল ড্রাইভে বা যেকোনো জায়গায় রাখা যায়। জায়গা লিংক করলে অ্যাপ নিজেই জানে ফাইল সেভ হয়েছে কি না। ফোন বদলালে বা অ্যাপ মুছলে সেই ফাইল থেকে সব ফিরিয়ে আনা যাবে।",
                "Keep the backup in a phone folder, Google Drive or anywhere else. When a place is linked, the app knows whether the file was saved. If you change phones or delete the app, restore everything from that file.");
        }
        catch (Exception ex) { AppLog.Error("Settings.Backup", ex); }
    }

    void BuildAboutSection()
    {
        AboutHost.Children.Clear();
        AboutHost.Add(new Label { Text = L.T("ℹ️ অ্যাপ সম্পর্কে", "ℹ️ About the app"), StyleClass = new[] { "H2" } });

        var v = new VerticalStackLayout { Spacing = 10 };
        v.Add(new Label
        {
            Text = L.T(AppInfo.AppNameBn, AppInfo.AppNameEn) + "  ·  v" + AppInfo.Version,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold
        });
        v.Add(new BoxView { HeightRequest = 1, Color = Ui.Line });
        v.Add(new Label { Text = L.T("ডেভেলপ করেছেন", "Developed by"), FontSize = 12, TextColor = Ui.Muted });

        foreach (var d in AppInfo.Developers)
        {
            if (string.IsNullOrWhiteSpace(d.Name)) continue;
            v.Add(DevRow(string.IsNullOrWhiteSpace(d.Icon) ? "👤" : d.Icon, d.Name, L.T(d.NoteBn, d.NoteEn)));
        }

        v.Add(new BoxView { HeightRequest = 1, Color = Ui.Line });
        v.Add(new Label
        {
            Text = L.T("ডেভেলপার সম্পর্কে বিস্তারিত ›", "More about the developer ›"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ui.Primary
        });

        var card = FUi.Card(v, null, 14);
        card.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => Ui.OpenModal(this, new DeveloperPage()))
        });
        AboutHost.Add(card);
    }

    static View DevRow(string icon, string name, string note)
    {
        var col = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        col.Add(new Label { Text = name, FontSize = 16, FontAttributes = FontAttributes.Bold });
        if (!string.IsNullOrWhiteSpace(note))
            col.Add(new Label { Text = note, FontSize = 12, TextColor = Ui.Muted });
        var g = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        g.Add(Ui.Badge(Ui.Primary, icon, 40), 0);
        g.Add(col, 1);
        return g;
    }
}
