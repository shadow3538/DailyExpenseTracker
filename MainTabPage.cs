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
                oldView.FadeTo(0, 80, Easing.CubicIn),
                newView.FadeTo(1, 140, Easing.CubicOut),
                newView.TranslateTo(0, 0, 140, Easing.CubicOut));

            oldView.IsVisible = false;
            oldView.Opacity = 1;
            oldView.TranslationX = 0;

            RootChrome.ActivateFor(newPage);
            if (!_activated[next])
            {
                _activated[next] = true;
                if (newPage is IHostActivatable activatable)
                    await activatable.ActivateForHostAsync();
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

    async Task WarmTabsAsync()
    {
        if (_warmStarted) return;
        _warmStarted = true;
        try
        {
            await Task.Delay(450);
            for (int i = 1; i < _pages.Length; i++)
            {
                if (_index != 0 || _activated[i]) break;

                var page = _pages[i];
                if (page is IHostActivatable activatable)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (_index == 0 && !_activated[i])
                        {
                            await activatable.ActivateForHostAsync();
                            _activated[i] = true;
                        }
                    });
                }

                await Task.Delay(250);
            }
        }
        catch (Exception ex) { AppLog.Error("TabHost.Warm", ex); }
    }

    public async Task RefreshAfterPreferenceChangeAsync()
    {
        try
        {
            var keepIndex = _index;
            _switching = true;

            if (RootChrome.Active is { IsOpen: true } chrome)
                await chrome.CloseAsync();

            foreach (var page in _pages)
            {
                if (page is IHostActivatable activatable)
                {
                    try { activatable.DeactivateForHost(); } catch { }
                }
            }

            _contentHost.Children.Clear();
            var fresh = new ContentPage[]
            {
                new HomePage(),
                new AddPage(),
                new ReportPage(),
                new CalculatorPage(),
                new SettingsPage()
            };

            for (int i = 0; i < fresh.Length; i++)
            {
                var view = fresh[i].Content ?? new Grid();
                fresh[i].Content = null;
                _pages[i] = fresh[i];
                _views[i] = view;
                view.IsVisible = i == keepIndex;
                view.Opacity = 1;
                view.TranslationX = 0;
                _contentHost.Add(view);
            }

            Array.Clear(_activated, 0, _activated.Length);
            _index = Math.Clamp(keepIndex, 0, _pages.Length - 1);
            Ui.CurrentRoute = _routes[_index];
            RootChrome.ActivateFor(_pages[_index]);

            if (_pages[_index] is IHostActivatable current)
            {
                _activated[_index] = true;
                await current.ActivateForHostAsync();
            }

            UpdateTabBar();
            _warmStarted = false;
            _ = WarmTabsAsync();
        }
        catch (Exception ex) { AppLog.Error("TabHost.Refresh", ex); }
        finally { _switching = false; }
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
