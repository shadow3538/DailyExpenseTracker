using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

// Profile view page
public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        ToolbarItems.Add(new ToolbarItem
        {
            Text = L.T("বন্ধ করুন"),
            Command = new Command(async () => await CloseAsync())
        });
    }

    void OnChanged() => _ = LoadAsync();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Profile.Changed -= OnChanged;
        Profile.Changed += OnChanged;
        Store.Changed -= OnChanged;
        Store.Changed += OnChanged;
        await LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Profile.Changed -= OnChanged;
        Store.Changed -= OnChanged;
    }

    // Back closes page
    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync();
        return true;
    }

    async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    void OnEdit(object? sender, EventArgs e) => Ui.OpenProfileEdit(this);

    async Task LoadAsync()
    {
        try
        {
            AvatarHost.Content = Ui.Avatar(104, ring: true);
            NameLabel.Text = Profile.Name.Length > 0 ? Profile.Name : L.T("নাম দেওয়া হয়নি");

            var sub = new List<string>();
            if (Profile.Occupation.Length > 0) sub.Add(Profile.Occupation);
            if (Profile.Age is int age) sub.Add(age.ToString(Fmt.Inv) + L.T(" বছর"));
            SubLabel.Text = sub.Count > 0 ? string.Join(" · ", sub) : L.T("প্রোফাইল এডিট করে তথ্য যোগ করুন");

            BioCard.IsVisible = Profile.Bio.Length > 0;
            BioLabel.Text = Profile.Bio;

            InfoStack.Children.Clear();
            var dob = Profile.Dob;
            InfoStack.Add(Row("📞", L.T("মোবাইল"), Profile.Phone));
            InfoStack.Add(Row("✉️", L.T("ইমেইল"), Profile.Email));
            InfoStack.Add(Row("💼", L.T("পেশা"), Profile.Occupation));
            InfoStack.Add(Row("🎂", L.T("জন্মতারিখ"), dob == null ? "" : dob.Value.ToString("dd MMMM yyyy", Fmt.Inv)));
            InfoStack.Add(Row("👤", L.T("লিঙ্গ"), L.T(Profile.Gender)));
            InfoStack.Add(Row("🩸", L.T("রক্তের গ্রুপ"), Profile.BloodGroup));
            InfoStack.Add(Row("📍", L.T("ঠিকানা"), Profile.Address));

            var st = await Store.GetStatsAsync();
            CountLabel.Text = st.Count.ToString(Fmt.Inv) + L.T("টি");
            TotalLabel.Text = Fmt.Money0(st.Total);
            SinceLabel.Text = st.First == null ? "—" : st.First.Value.ToString("dd MMM yyyy", Fmt.Inv);
        }
        catch { }
    }

    // Info row: icon
    static View Row(string icon, string title, string value)
    {
        var has = !string.IsNullOrWhiteSpace(value);

        var badge = new Border
        {
            WidthRequest = 38,
            HeightRequest = 38,
            StrokeThickness = 0,
            BackgroundColor = Ui.PrimarySoft,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(19) },
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = icon,
                FontSize = 17,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var text = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        text.Add(new Label { Text = title, FontSize = 12, TextColor = Ui.Muted });
        text.Add(new Label
        {
            Text = has ? value : L.T("যোগ করা হয়নি"),
            FontSize = 16,
            TextColor = has ? Ui.Ink : Ui.Muted,
            FontAttributes = has ? FontAttributes.Bold : FontAttributes.None
        });

        var g = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        g.Add(badge, 0);
        g.Add(text, 1);
        return g;
    }
}
