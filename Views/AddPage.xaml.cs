using Microsoft.Maui.Controls.Shapes;
using DailyExpenseTracker.Data;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public partial class AddPage : ContentPage
{
    string _rowsKey = "";
    List<ExpenseCategory> _cats = new();
    List<Choice> _choices = new();
    ExpenseCategory? _sel;
    int _toastId;
    bool _appeared;
    bool _pickedPast;
    bool _saving;

    public AddPage()
    {
        InitializeComponent();
        Store.Changed -= OnStoreChanged;
        TabSwipe.Guard(CatScroll);
        Track(NoteEntry, NoteEntry);
        Track(WhatEntry, WhatEntry);
        Track(FreeAmount, FreeAmount);
        RootChrome.Attach(this);
    }

    View? _focusView;

    void Track(Entry e, View row)
    {
        e.Focused += (_, _) => { _focusView = row; _ = ScrollToAsync(row); };
        e.Unfocused += (_, _) => { if (_focusView == row) _focusView = null; };
    }

    async Task ScrollToAsync(View v)
    {
        try
        {
            await Task.Delay(230);
            await MainScroll.ScrollToAsync(v, ScrollToPosition.Center, true);
        }
        catch { }
    }

    void OnKeyboard()
    {
        MainStack.Padding = new Thickness(16, 14, 16, KeyboardWatcher.Visible ? KeyboardWatcher.HeightDp + 30 : 90);
        if (KeyboardWatcher.Visible && _focusView != null) _ = ScrollToAsync(_focusView);
    }

    void OnStoreChanged() { if (_appeared) _ = LoadEntriesAsync(); }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _appeared = true;
        Store.Changed -= OnStoreChanged;
        Store.Changed += OnStoreChanged;
        KeyboardWatcher.Changed -= OnKeyboard;
        KeyboardWatcher.Changed += OnKeyboard;
        try
        {

            if (Ui.PendingAddDate is DateTime pd)
            {
                Ui.PendingAddDate = null;
                DatePick.MaximumDate = DateTime.Today;
                DatePick.Date = pd.Date > DateTime.Today ? DateTime.Today : pd.Date;
                _pickedPast = DatePick.Date.Date < DateTime.Today;
            }

            DatePick.MaximumDate = DateTime.Today;
            if (!_pickedPast && DatePick.Date.Date != DateTime.Today) DatePick.Date = DateTime.Today;
            RefreshDateUi();

            await LoadCategoriesAsync();
            await LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            AppLog.Error("Add.Appearing", ex);
            await ShowToast(L.T("পেজ লোড করা যায়নি"), false);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _appeared = false;
        Store.Changed -= OnStoreChanged;
        KeyboardWatcher.Changed -= OnKeyboard;
    }

    DateTime SelectedDay => DatePick.Date.Date;

    void OnDateSelected(object? sender, DateChangedEventArgs e)
    {
        _pickedPast = e.NewDate.Date < DateTime.Today;
        RefreshDateUi();
        _ = LoadEntriesAsync();
    }

    void RefreshDateUi()
    {
        DayHint.Text = Fmt.DayHint(SelectedDay);
        var canNext = SelectedDay < DateTime.Today;
        NextDayBtn.IsEnabled = canNext;
        NextDayBtn.Opacity = canNext ? 1 : 0.3;
    }

    void OnPrevDay(object? sender, EventArgs e) => DatePick.Date = SelectedDay.AddDays(-1);

    void OnNextDay(object? sender, EventArgs e)
    {
        if (SelectedDay < DateTime.Today) DatePick.Date = SelectedDay.AddDays(1);
    }

    async Task LoadCategoriesAsync()
    {
        _cats = await Store.GetCategoriesAsync();
        _choices = await Store.GetChoicesAsync();

        var last = _sel?.Name ?? Preferences.Default.Get("add_cat", "");
        _sel = _cats.FirstOrDefault(c => c.Name == last) ?? _cats.FirstOrDefault();

        BuildCatBar();
        BuildPanel(force: true);
    }

    void BuildCatBar()
    {
        CatBar.Children.Clear();
        foreach (var cat in _cats)
        {
            var selected = _sel != null && cat.Id == _sel.Id;
            var c = Ui.ItemColor(cat.Name);
            var box = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            box.Add(new Label { Text = cat.Icon, FontSize = 26, HorizontalTextAlignment = TextAlignment.Center });
            box.Add(new Label
            {
                Text = cat.Name,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                TextColor = selected ? Colors.White : Ui.Ink
            });

            var tile = new Border
            {
                WidthRequest = 88,
                HeightRequest = 80,
                Padding = new Thickness(4, 6),
                BackgroundColor = selected ? c : Ui.Surface,
                Stroke = selected ? c : Ui.Line,
                StrokeThickness = 1.5,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Content = box
            };
            var target = cat;
            tile.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (_sel?.Id == target.Id) return;
                    _sel = target;
                    Preferences.Default.Set("add_cat", target.Name);
                    BuildCatBar();
                    BuildPanel(force: true);
                    _ = PulseSelectedCategoryAsync();
                })
            });
            CatBar.Children.Add(tile);
        }
    }

    async Task PulseSelectedCategoryAsync()
    {
        try
        {
            await Task.Delay(20);
            var index = _cats.FindIndex(c => _sel != null && c.Id == _sel.Id);
            if (index >= 0 && index < CatBar.Children.Count && CatBar.Children[index] is Border tile)
                await Ui.TapPulseAsync(tile);
        }
        catch (Exception ex) { AppLog.Error("Add.CategoryAnimation", ex); }
    }

    void BuildPanel(bool force = false)
    {
        var cat = _sel;
        if (cat == null)
        {
            PanelCard.IsVisible = false;
            return;
        }
        PanelCard.IsVisible = true;

        var color = Ui.ItemColor(cat.Name);
        PanelCard.Stroke = color.WithAlpha(0.5f);
        PanelCard.StrokeThickness = 1.5;
        PanelBadge.Content = Ui.Badge(color, cat.Icon, 42);
        PanelTitle.Text = cat.Name;

        FreeBox.IsVisible = cat.IsFreeText;
        OptionsBox.IsVisible = !cat.IsFreeText;

        if (cat.IsFreeText)
        {
            PanelHint.Text = L.T("লিখে টাকা দিয়ে যোগ করুন");
            WhatEntry.Placeholder = string.IsNullOrWhiteSpace(cat.Hint) ? L.T("কী বাবদ খরচ?") : cat.Hint;
            FreeAddBtn.BackgroundColor = color;
            return;
        }

        PanelHint.Text = L.T("যেটার খরচ, তার পাশে টাকা লিখে 'যোগ' চাপুন");
        var names = _choices.Where(c => c.CategoryId == cat.Id).Select(c => c.Name).ToList();
        var key = cat.Id + ":" + string.Join("|", names);
        if (!force && key == _rowsKey) return;
        _rowsKey = key;

        RowsStack.Children.Clear();
        if (names.Count == 0)
        {
            RowsStack.Add(new Label { Text = L.T("এই ক্যাটাগরিতে কোনো অপশন নেই। সেটিংস থেকে অপশন যোগ করুন।"), TextColor = Ui.Muted });
            return;
        }
        foreach (var name in names) RowsStack.Add(BuildRow(name, color));
    }

    View BuildRow(string name, Color color)
    {
        var entry = new Entry
        {
            Placeholder = L.T("৳ টাকা"),
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.End,
            WidthRequest = 110,
            VerticalOptions = LayoutOptions.Center
        };
        var btn = new Button
        {
            Text = L.T("যোগ"),
            WidthRequest = 72,
            HeightRequest = 44,
            BackgroundColor = color,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center
        };
        btn.Clicked += async (_, _) => await AddAsync(name, entry);
        entry.Completed += async (_, _) => await AddAsync(name, entry);

        var g = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        g.Add(new Label { Text = name, FontSize = 17, VerticalOptions = LayoutOptions.Center }, 0);
        g.Add(entry, 1);
        g.Add(btn, 2);

        var rowBox = new Border
        {
            Content = g,
            Padding = new Thickness(14, 6),
            BackgroundColor = color.WithAlpha(0.07f),
            Stroke = color.WithAlpha(0.25f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) }
        };
        Track(entry, rowBox);
        return rowBox;
    }

    async Task AddAsync(string name, Entry entry)
    {
        if (_saving) return;
        _saving = true;
        try
        {
            if (!Fmt.TryAmount(entry.Text, out var amt) || amt <= 0)
            {
                entry.Focus();
                await ShowToast(L.T("টাকার অঙ্ক লিখুন"), false);
                return;
            }
            var day = SelectedDay;
            var isToday = day == DateTime.Today;
            await Store.AddAsync(new Expense
            {

                Date = isToday ? DateTime.Now : day + DateTime.Now.TimeOfDay,
                Item = name,
                Category = _sel?.Name ?? "",
                Amount = amt,
                Note = (NoteEntry.Text ?? "").Trim()
            });
            entry.Text = "";
            NoteEntry.Text = "";

            var msg = name + ": " + Fmt.Money(amt) + L.T(" যোগ হয়েছে");
            if (!isToday) msg += " (" + day.ToString("dd MMM", Fmt.Inv) + ")";
            await ShowToast(msg, true);
        }
        catch (Exception ex)
        {
            AppLog.Error("Add.Expense", ex);
            await ShowToast(L.T("খরচ যোগ করা যায়নি"), false);
        }
        finally { _saving = false; }
    }

    async void OnFreeAdd(object? sender, EventArgs e)
    {
        if (_saving) return;
        _saving = true;
        try
        {
            var cat = _sel;
            if (cat == null) return;

            var what = (WhatEntry.Text ?? "").Trim();
            if (what.Length == 0)
            {
                WhatEntry.Focus();
                await ShowToast(L.T("কী বাবদ খরচ তা লিখুন"), false);
                return;
            }
            if (!Fmt.TryAmount(FreeAmount.Text, out var amt) || amt <= 0)
            {
                FreeAmount.Focus();
                await ShowToast(L.T("টাকার অঙ্ক লিখুন"), false);
                return;
            }

            var day = SelectedDay;
            var isToday = day == DateTime.Today;
            await Store.AddAsync(new Expense
            {
                Date = isToday ? DateTime.Now : day + DateTime.Now.TimeOfDay,
                Item = cat.Name,
                Category = cat.Name,
                Amount = amt,
                Note = what
            });
            WhatEntry.Text = "";
            FreeAmount.Text = "";
            FreeAmount.Unfocus();
            WhatEntry.Unfocus();
            var msg = cat.Name + " · " + what + ": " + Fmt.Money(amt) + L.T(" যোগ হয়েছে");
            if (!isToday) msg += " (" + day.ToString("dd MMM", Fmt.Inv) + ")";
            await ShowToast(msg, true);
        }
        catch (Exception ex)
        {
            AppLog.Error("Add.FreeExpense", ex);
            await ShowToast(L.T("খরচ যোগ করা যায়নি"), false);
        }
        finally { _saving = false; }
    }

    async Task ShowToast(string text, bool ok)
    {
        var id = ++_toastId;
        ToastLabel.Text = text;
        ToastBox.BackgroundColor = ok ? Ui.Green : Ui.Red;
        ToastBox.Opacity = 1;
        ToastBox.IsVisible = true;
        await Task.Delay(1800);
        if (id != _toastId) return;
        await ToastBox.FadeTo(0, 250);
        if (id == _toastId) ToastBox.IsVisible = false;
    }

    async Task LoadEntriesAsync()
    {
        try
        {
            var day = SelectedDay;
            var isToday = day == DateTime.Today;
            var list = await Store.GetRangeAsync(day, day.AddDays(1));
            TodayHeader.Text = (isToday ? L.T("আজকের এন্ট্রি") : Fmt.DayTitle(day)) + " · " + Fmt.Money(list.Sum(x => x.Amount));

            EntriesStack.Children.Clear();
            if (list.Count == 0)
            {
                EntriesStack.Add(new Label
                {
                    Text = isToday ? L.T("আজ এখনো কোনো খরচ যোগ হয়নি") : L.T("এই দিনে কোনো খরচ যোগ হয়নি"),
                    TextColor = Ui.Muted
                });
                return;
            }
            foreach (var item in list)
                EntriesStack.Add(Ui.EntryRow(item, x => Ui.OpenEdit(this, x)));
        }
        catch (Exception ex)
        {
            AppLog.Error("Add.LoadEntries", ex);
        }
    }
}
