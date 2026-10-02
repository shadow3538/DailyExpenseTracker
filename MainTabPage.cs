using Microsoft.Maui.Controls.Shapes;

namespace DailyExpenseTracker;

public interface IHostActivatable
{
    Task ActivateForHostAsync();
    void DeactivateForHost();
}

public sealed class MainTabPage : ContentPage
{
    public static MainTabPage? Current { get; private set; }

    readonly ContentPage[] _pages;
    readonly View[] _views;
    readonly Grid _contentHost;
    readonly Grid _tabBar;
    readonly Image[] _icons = new Image[5];
    readonly Label[] _labels = new Label[5];
    readonly string[] _routes = { "home", "add", "report", "calc", "settings" };
    readonly string[] _iconFiles = { "tab_home.svg", "tab_add.svg", "tab_report.svg", "tab_calc.svg", "tab_settings.svg" };
    readonly string[] _bn = { "হোম", "যোগ করুন", "রিপোর্ট", "ক্যালকুলেটর", "সেটিংস" };
    readonly string[] _en = { "Home", "Add", "Report", "Calculator", "Settings" };

    int _index;
    bool _switching;
    bool _warmStarted;
    readonly bool[] _activated = new bool[5];
    readonly bool[] _warming = new bool[5];
    int _priorityIndex = -1;

    public MainTabPage()
    {
        Title = L.T("পকেটনামা", "Pocket Nama");
        Current = this;

        _pages = new ContentPage[]
        {
            new HomePage(),
            new AddPage(),
            new ReportPage(),
            new CalculatorPage(),
            new SettingsPage()
        };

        _views = new View[_pages.Length];
        _contentHost = new Grid { BackgroundColor = Ui.PageBg };
        for (int i = 0; i < _pages.Length; i++)
        {
            var view = _pages[i].Content ?? new Grid();
            _pages[i].Content = null;
            _views[i] = view;
            view.IsVisible = i == 0;
            _contentHost.Add(view);
        }

        _tabBar = BuildTabBar();
        Content = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            Children = { _contentHost, _tabBar }
        };
        Grid.SetRow(_contentHost, 0);
        Grid.SetRow(_tabBar, 1);

        UpdateTabBar();
        KeyboardWatcher.Changed += OnKeyboardChanged;
        Loaded += async (_, _) => await ActivateCurrentAsync();
    }

    Grid BuildTabBar()
    {
        var grid = new Grid
        {
            HeightRequest = 66,
            Padding = new Thickness(6, 4, 6, 5),
            ColumnSpacing = 2,
            BackgroundColor = Ui.Surface
        };
        for (int i = 0; i < 5; i++) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int i = 0; i < 5; i++)
        {
            int n = i;
            var icon = new Image { Source = _iconFiles[i], WidthRequest = 23, HeightRequest = 23, Aspect = Aspect.AspectFit };
            var label = new Label { FontSize = 10, HorizontalTextAlignment = TextAlignment.Center, MaxLines = 1 };
            _icons[i] = icon;
            _labels[i] = label;

            var box = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, Padding = new Thickness(0, 2) };
            box.Add(icon);
            box.Add(label);
            box.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _ = SwitchToAsync(n)) });
            grid.Add(box, i);
        }
        return grid;
    }

    void OnKeyboardChanged()
    {
        try { _tabBar.IsVisible = !KeyboardWatcher.Visible; } catch { }
    }

    public Task SwitchToAsync(string route)
    {
        var i = Array.IndexOf(_routes, route);
        return i < 0 ? Task.CompletedTask : SwitchToAsync(i);
    }

    public async Task SwitchToAsync(int next)
    {
        if (next < 0 || next >= _pages.Length || next == _index || _switching) return;

        // If a hidden page is requested before the background warm pass reaches it,
        // move that page to the front of the warm queue.  The user-selected page
        // always gets priority over the remaining background pages.
        if (!_activated[next])
            _priorityIndex = next;

        _switching = true;
        try
        {
            var oldView = _views[_index];
            var newView = _views[next];
            var oldPage = _pages[_index];
            var newPage = _pages[next];
            var forward = next > _index;

            Ui.PrevRoute = _routes[_index];
            Ui.CurrentRoute = _routes[next];
            _index = next;

            newView.IsVisible = true;
            newView.Opacity = 0;
            newView.TranslationX = forward ? 8 : -8;

            await Task.WhenAll(
                oldView.FadeTo(0, 110, Easing.CubicIn),
                newView.FadeTo(1, 220, Easing.CubicOut),
                newView.TranslateTo(0, 0, 220, Easing.CubicOut));

            oldView.IsVisible = false;
            oldView.Opacity = 1;
            oldView.TranslationX = 0;

            RootChrome.ActivateFor(newPage);
            if (!_activated[next])
            {
                if (_warming[next])
                {
                    // The background warm pass is already preparing this page.
                    // Let that same activation finish instead of starting a second one.
                    while (_warming[next])
                        await Task.Yield();
                }

                if (!_activated[next] && newPage is IHostActivatable activatable)
                {
                    await activatable.ActivateForHostAsync();
                    _activated[next] = true;
                }
                _priorityIndex = -1;
            }
            UpdateTabBar();
        }
        catch (Exception ex) { AppLog.Error("TabHost.Switch", ex); }
        finally { _switching = false; }
    }

    async Task ActivateCurrentAsync()
    {
        try
        {
            Ui.CurrentRoute = _routes[_index];
            RootChrome.ActivateFor(_pages[_index]);
            if (!_activated[_index])
            {
                _activated[_index] = true;
                if (_pages[_index] is IHostActivatable activatable)
                    await activatable.ActivateForHostAsync();
            }
            _ = WarmTabsAsync();
        }
        catch (Exception ex) { AppLog.Error("TabHost.Start", ex); }
    }

    async Task WarmTabsAsync(int[]? preferredOrder = null)
    {
        if (_warmStarted) return;
        _warmStarted = true;
        try
        {
            // Start immediately after Home activation. There is intentionally no
            // artificial delay between hidden pages. The queue yields naturally
            // whenever ActivateForHostAsync performs async I/O.
            var pending = (preferredOrder ?? new[] { 1, 2, 3, 4 }).Distinct().ToList();

            while (pending.Count > 0)
            {
                int i;
                if (_priorityIndex >= 1 && _priorityIndex < _pages.Length && !_activated[_priorityIndex])
                {
                    i = _priorityIndex;
                    pending.Remove(i);
                    _priorityIndex = -1;
                }
                else
                {
                    i = pending[0];
                    pending.RemoveAt(0);
                }

                if (_activated[i] || _warming[i]) continue;

                var page = _pages[i];
                if (page is not IHostActivatable activatable) continue;

                _warming[i] = true;
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (!_activated[i])
                        {
                            await activatable.ActivateForHostAsync();
                            _activated[i] = true;
                        }
                    });
                }
                finally
                {
                    _warming[i] = false;
                }

                // A tab selected while another page was warming becomes the next
                // page in line, without introducing a timer-based delay.
                if (_priorityIndex >= 1 && _priorityIndex < _pages.Length &&
                    !_activated[_priorityIndex] && !pending.Contains(_priorityIndex))
                {
                    pending.Insert(0, _priorityIndex);
                    _priorityIndex = -1;
                }
            }
        }
        catch (Exception ex) { AppLog.Error("TabHost.Warm", ex); }
    }

    public async Task RefreshAfterPreferenceChangeAsync()
    {
        try
        {
            if (RootChrome.Active is { IsOpen: true } chrome)
                await chrome.CloseAsync();

            // Keep the existing page instances alive. Refresh only their data/UI
            // in place so changing language/theme never recreates the tab pages.
            foreach (var page in _pages)
            {
                if (page is IHostActivatable activatable)
                {
                    try { activatable.DeactivateForHost(); } catch { }
                }
            }

            Array.Clear(_activated, 0, _activated.Length);
            Array.Clear(_warming, 0, _warming.Length);
            _priorityIndex = -1;

            var keepIndex = Math.Clamp(_index, 0, _pages.Length - 1);
            Ui.CurrentRoute = _routes[keepIndex];
            RootChrome.ActivateFor(_pages[keepIndex]);

            if (_pages[keepIndex] is IHostActivatable current)
            {
                await current.ActivateForHostAsync();
                _activated[keepIndex] = true;
            }

            UpdateTabBar();

            // Preference changes warm Settings first, then Home, then the remaining
            // tabs. There is no timer delay; the next page starts as soon as the
            // previous activation completes.
            _warmStarted = false;
            _ = WarmTabsAsync(new[] { 4, 0, 1, 2, 3 });
        }
        catch (Exception ex) { AppLog.Error("TabHost.Refresh", ex); }
    }

    void UpdateTabBar()
    {
        for (int i = 0; i < 5; i++)
        {
            var active = i == _index;
            _labels[i].Text = L.IsEn ? _en[i] : _bn[i];
            _labels[i].TextColor = active ? Ui.Primary : Ui.Muted;
            _labels[i].FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
            _icons[i].Opacity = active ? 1 : 0.55;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (RootChrome.Active is { IsOpen: true } chrome)
            {
                _ = chrome.CloseAsync();
                return true;
            }
            if (_index != 0)
            {
                _ = SwitchToAsync(0);
                return true;
            }
        }
        catch { }
        return base.OnBackButtonPressed();
    }
}
