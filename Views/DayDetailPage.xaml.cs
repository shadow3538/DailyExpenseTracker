using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

// Day detail: total
public partial class DayDetailPage : ContentPage
{
    DateTime _day;

    static readonly Color OnDarkTrack = new Color(1f, 1f, 1f, 0.25f);
    static readonly Color OnDarkWarn = Color.FromArgb("#FFD180");
    static readonly Color OnDarkOver = Color.FromArgb("#FF8A80");
    static readonly Color OnDarkOverText = Color.FromArgb("#FFD2CB");

    public DayDetailPage(DateTime day)
    {
        InitializeComponent();
        _day = day.Date;
        ToolbarItems.Add(new ToolbarItem
        {
            Text = L.T("বন্ধ করুন"),
            Command = new Command(async () => await CloseAsync())
        });
    }

    void OnStoreChanged() => _ = LoadAsync();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;
        await LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Store.Changed -= OnStoreChanged;
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

    async void OnPrevDay(object? sender, EventArgs e)
    {
        _day = _day.AddDays(-1);
        await LoadAsync();
    }

    async void OnNextDay(object? sender, EventArgs e)
    {
        if (_day >= DateTime.Today) return;
        _day = _day.AddDays(1);
        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            var list = await Store.GetRangeAsync(_day, _day.AddDays(1));
            var sum = list.Sum(x => x.Amount);
            var dl = Period.DailyLimitFor(_day);
            bool isToday = _day == DateTime.Today;

            NextBtn.IsEnabled = _day < DateTime.Today;
            NextBtn.Opacity = NextBtn.IsEnabled ? 1 : 0.3;

            DayLabel.Text = Fmt.DayTitle(_day);
            TotalLabel.Text = Fmt.Money0(sum);

            if (dl > 0)
            {
                double frac = (double)(sum / dl);
                var fill = frac >= 1 ? OnDarkOver : frac >= 0.75 ? OnDarkWarn : Colors.White;
                BarHost.Content = Ui.Bar(frac, fill, OnDarkTrack, 12);
                var diff = dl - sum;
                if (diff >= 0)
                {
                    StatusLabel.Text = L.T("লিমিট ") + Fmt.Money0(dl) + " · " + Fmt.Money0(diff) + (isToday ? L.T(" বাকি") : L.T(" কম খরচ"));
                    StatusLabel.TextColor = Colors.White;
                }
                else
                {
                    StatusLabel.Text = L.T("লিমিট ") + Fmt.Money0(dl) + " · " + Fmt.Money0(-diff) + L.T(" বেশি খরচ");
                    StatusLabel.TextColor = OnDarkOverText;
                }
            }
            else
            {
                BarHost.Content = null;
                StatusLabel.Text = L.T("দৈনিক লিমিট সেট করা নেই");
                StatusLabel.TextColor = Colors.White;
            }

            // Category tags
            ChipsBox.Children.Clear();
            foreach (var grp in list.GroupBy(x => x.Cat)
                                    .Select(g => new { Name = g.Key, Sum = g.Sum(x => x.Amount) })
                                    .OrderByDescending(x => x.Sum))
            {
                var c = Ui.ItemColor(grp.Name);
                var chip = Ui.Chip(Ui.ItemIcon(grp.Name) + " " + grp.Name + " " + Fmt.Money0(grp.Sum), c.WithAlpha(0.16f), Ui.Ink, 13);
                chip.Margin = new Thickness(0, 0, 6, 6);
                ChipsBox.Children.Add(chip);
            }
            ChipsBox.IsVisible = list.Count > 0;

            ListTitle.Text = L.T("এন্ট্রি (") + list.Count.ToString(Fmt.Inv) + L.T("টি)");
            ListHint.IsVisible = list.Count > 0;
            EntriesStack.Children.Clear();
            if (list.Count == 0)
            {
                EntriesStack.Add(new Label { Text = L.T("এই দিনে কোনো খরচ নেই"), TextColor = Ui.Muted });
                return;
            }
            foreach (var item in list)
                EntriesStack.Add(Ui.EntryRow(item, x => Ui.OpenEdit(this, x)));
        }
        catch { }
    }

    async void OnAdd(object? sender, EventArgs e)
    {
        try
        {
            Ui.PendingAddDate = _day;
            await Navigation.PopModalAsync();
            await Ui.GoTo("add");
        }
        catch { }
    }
}
