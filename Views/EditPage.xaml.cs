using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public partial class EditPage : ContentPage
{
    readonly Expense _e;
    string _item;
    string _cat;
    List<ExpenseCategory> _cats = new();
    List<Choice> _choices = new();

    public EditPage(Expense e)
    {
        InitializeComponent();
        _e = e;
        _item = string.IsNullOrWhiteSpace(e.Item) ? "Extra" : e.Item;
        _cat = e.Cat;

        ToolbarItems.Add(new ToolbarItem
        {
            Text = L.T("বন্ধ করুন"),
            Command = new Command(async () => await CloseAsync())
        });

        AmountEntry.Text = e.Amount.ToString("0.##", Fmt.Inv);
        NoteEntry.Text = e.Note;
        DatePick.MaximumDate = DateTime.Today;
        DatePick.Date = e.Date.Date > DateTime.Today ? DateTime.Today : e.Date.Date;
        TimePick.Time = e.Date.TimeOfDay;
        RefreshDateUi();
        _ = LoadChoicesAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync();
        return true;
    }

    async Task LoadChoicesAsync()
    {
        try
        {
            _cats = await Store.GetCategoriesAsync();
            _choices = await Store.GetChoicesAsync();

            if (_cats.All(c => c.Name != _cat))
                _cats.Add(new ExpenseCategory { Id = -1, Name = _cat, Icon = Ui.ItemIcon(_cat), Color = "#00897B", IsFreeText = _item == _cat });
            BuildChips();
        }
        catch { }
    }

    ExpenseCategory? CurCat => _cats.FirstOrDefault(c => c.Name == _cat);

    void BuildChips()
    {

        CatBox.Children.Clear();
        foreach (var cat in _cats)
        {
            var selected = cat.Name == _cat;
            var c = Ui.ItemColor(cat.Name);
            var chip = MakeChip(cat.Icon + " " + cat.Name, selected, c);
            var target = cat;
            chip.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (_cat != target.Name)
                    {
                        _cat = target.Name;
                        if (target.IsFreeText) _item = target.Name;
                        else
                        {
                            var opts = _choices.Where(x => x.CategoryId == target.Id).ToList();
                            if (!opts.Any(x => x.Name == _item)) _item = opts.FirstOrDefault()?.Name ?? target.Name;
                        }
                    }
                    BuildChips();
                })
            });
            CatBox.Children.Add(chip);
        }

        ChipsBox.Children.Clear();
        var cur = CurCat;
        bool free = cur == null || cur.IsFreeText;
        OptTitle.IsVisible = !free;
        ChipsBox.IsVisible = !free;
        NoteTitle.Text = free
            ? (string.IsNullOrWhiteSpace(cur?.Hint) ? L.T("কী বাবদ খরচ (লিখতে হবে)") : cur!.Hint)
            : L.T("নোট (ঐচ্ছিক)");

        if (free) return;

        var names = _choices.Where(x => x.CategoryId == cur!.Id).Select(x => x.Name).ToList();
        if (!names.Contains(_item)) names.Add(_item);
        var color = Ui.ItemColor(cur!.Name);
        foreach (var name in names)
        {
            var chip = MakeChip(Ui.ItemIcon(name) + " " + name, name == _item, color);
            var n = name;
            chip.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => { _item = n; BuildChips(); })
            });
            ChipsBox.Children.Add(chip);
        }
    }

    static Border MakeChip(string text, bool selected, Color c) => new Border
    {
        Padding = new Thickness(14, 9),
        Margin = new Thickness(0, 0, 8, 8),
        BackgroundColor = selected ? c : Ui.Gray,
        Stroke = selected ? c : Ui.Line,
        StrokeThickness = 1,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(20) },
        Content = new Label
        {
            Text = text,
            FontSize = 15,
            FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
            TextColor = selected ? Colors.White : Ui.Ink
        }
    };

    void RefreshDateUi()
    {
        var d = DatePick.Date.Date;
        DayHint.Text = Fmt.DayHint(d);
        NextDayBtn.IsEnabled = d < DateTime.Today;
        NextDayBtn.Opacity = NextDayBtn.IsEnabled ? 1 : 0.3;
    }

    void OnDateSelected(object? sender, DateChangedEventArgs e) => RefreshDateUi();

    void OnPrevDay(object? sender, EventArgs e) => DatePick.Date = DatePick.Date.Date.AddDays(-1);

    void OnNextDay(object? sender, EventArgs e)
    {
        if (DatePick.Date.Date < DateTime.Today) DatePick.Date = DatePick.Date.Date.AddDays(1);
    }

    async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    async void OnSave(object? sender, EventArgs ev)
    {
        try
        {
            if (!Fmt.TryAmount(AmountEntry.Text, out var amt) || amt <= 0)
            {
                await DisplayAlert(L.T("সমস্যা"), L.T("সঠিক টাকার অঙ্ক লিখুন"), L.T("ঠিক আছে"));
                return;
            }
            var cur = CurCat;
            var note = (NoteEntry.Text ?? "").Trim();
            if (cur != null && cur.IsFreeText && note.Length == 0)
            {
                await DisplayAlert(L.T("সমস্যা"), L.T("কী বাবদ খরচ তা লিখুন"), L.T("ঠিক আছে"));
                return;
            }
            _e.Category = _cat;
            _e.Item = (cur != null && cur.IsFreeText) ? _cat : _item;
            _e.Amount = amt;
            _e.Note = note;
            _e.Date = DatePick.Date.Date + TimePick.Time;
            await Store.UpdateAsync(_e);
            Ui.Toast(L.T("✔ খরচ সেভ হয়েছে"));
            await CloseAsync();
        }
        catch { }
    }

    async void OnDelete(object? sender, EventArgs ev)
    {
        try
        {
            var ok = await DisplayAlert(L.T("মুছবেন?"), _e.Item + " · " + Fmt.Money0(_e.Amount), L.T("মুছুন"), L.T("না"));
            if (!ok) return;
            await Store.DeleteAsync(_e);
            Ui.Toast(L.T("খরচ মুছে ফেলা হয়েছে"));
            await CloseAsync();
        }
        catch { }
    }
}
