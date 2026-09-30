using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;

namespace DailyExpenseTracker;

// Custom top bar
public sealed class RootChrome
{
    // Chrome of the
    public static RootChrome? Active;

    // Attach to a
    public static RootChrome Attach(ContentPage page, View? titleView = null) => new RootChrome(page, titleView);

    readonly ContentPage _page;
    readonly Grid _root;
    readonly Grid _push;
    readonly BoxView _scrim;
    readonly Border _bar;
    readonly VerticalStackLayout _drawer;
    readonly Grid _drawerHost;
    double _inset;     // status bar padding (dp)
    double _w = 220;   // drawer width
    bool _anim;

    public bool IsOpen { get; private set; }
    public bool IsBusy => _anim;

    RootChrome(ContentPage page, View? titleView)
    {
        _page = page;
        Shell.SetNavBarIsVisible(page, false);

        var inner = page.Content;
        page.Content = null;

        // top bar
        _bar = BuildBar(titleView);

        // pushed layer: bar
        _push = new Grid
        {
            RowSpacing = 0,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        _push.SetDynamicResource(VisualElement.BackgroundColorProperty, "Bg");
        _push.Add(_bar, 0, 0);
        if (inner != null) _push.Add(inner, 0, 1);

        _scrim = new BoxView { Color = Colors.Black, Opacity = 0, IsVisible = false };
        Grid.SetRowSpan(_scrim, 2);
        _scrim.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _ = CloseAsync()) });
        _push.Add(_scrim, 0, 0);

        // drawer sits under
        _drawer = new VerticalStackLayout { Spacing = 2 };
        _drawerHost = new Grid
        {
            HorizontalOptions = LayoutOptions.Start,
            IsVisible = false,
            BackgroundColor = Ui.Surface,
            WidthRequest = _w
        };
        _drawerHost.Add(new ScrollView { Content = _drawer });

        _root = new Grid { BackgroundColor = Ui.Surface, IsClippedToBounds = true };
        _root.Add(_drawerHost);
        _root.Add(_push);
        page.Content = _root;

        page.Appearing += (_, _) =>
        {
            Active = this;
            Later(() => MeasureInset());
        };
        page.Disappearing += (_, _) =>
        {
            if (Active == this) Active = null;
            ForceClose();
        };
    }

    void Later(Action a)
    {
        try { _page.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(60), a); } catch { }
    }

    // top bar

    Border BuildBar(View? titleView)
    {
        var menu = new Label
        {
            Text = "☰",
            FontSize = 26,
            TextColor = Colors.White,
            WidthRequest = 48,
            HeightRequest = 48,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center
        };
        menu.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _ = OpenAsync()) });

        View title = titleView ?? new Label
        {
            Text = _page.Title,
            TextColor = Colors.White,
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var row = new Grid
        {
            HeightRequest = 56,
            Padding = new Thickness(4, 0, 8, 0),
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        row.Add(menu, 0);
        row.Add(title, 1);

        return new Border
        {
            Content = row,
            StrokeThickness = 0,
            BackgroundColor = Ui.NavBar,
            Padding = new Thickness(0)
        };
    }

    // Pad bar below
    void MeasureInset()
    {
        try
        {
            if (_bar.Handler?.PlatformView is not global::Android.Views.View v) return;
            var loc = new int[2];
            v.GetLocationOnScreen(loc);
            var res = v.Resources;
            if (res == null) return;
            int id = res.GetIdentifier("status_bar_height", "dimen", "android");
            int sb = id > 0 ? res.GetDimensionPixelSize(id) : 0;
            float dens = res.DisplayMetrics?.Density ?? 1f;
            double dp = Math.Max(0, sb - loc[1]) / dens;
            if (Math.Abs(dp - _inset) < 0.5) return;
            _inset = dp;
            _bar.Padding = new Thickness(0, dp, 0, 0);
        }
        catch (Exception ex) { AppLog.Error("Chrome.Inset", ex); }
    }

    // drawer

    void BuildDrawer()
    {
        _drawer.Children.Clear();
        _drawer.Padding = new Thickness(12, _inset + 14, 10, 16);

        // header: avatar +
        var texts = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        texts.Add(new Label
        {
            Text = Profile.Name.Length > 0 ? Profile.Name : L.T("প্রোফাইল সেট করুন", "Set up profile"),
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 2
        });
        texts.Add(new Label { Text = L.T(AppInfo.AppNameBn, AppInfo.AppNameEn), FontSize = 12, TextColor = Ui.Muted });
        var head = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(4, 0, 0, 10) };
        head.Add(Ui.Avatar(56));
        head.Add(texts);
        _drawer.Add(head);
        _drawer.Add(new BoxView { HeightRequest = 1, Color = Ui.Line, Margin = new Thickness(0, 0, 0, 8) });

        var route = Ui.CurrentRoute ?? "home";
        AddItem("👤", L.T("আমার প্রোফাইল", "My profile"), null, false, () => Ui.OpenProfile(_page));
        AddItem("🌐", L.T("ভাষা", "Language"), L.IsEn ? "English" : "বাংলা", false, () => Ui.OpenModal(_page, new LanguagePage()));
        AddItem("🎨", L.T("থিম", "Theme"), ThemeName(), false, () => Ui.OpenModal(_page, new ThemePage()));
        AddItem("🧮", L.T("ক্যালকুলেটর", "Calculator"), null, route == "calc", () => _ = Ui.GoTo("calc"));
        AddItem("⚙️", L.T("সেটিংস", "Settings"), null, route == "settings", () => _ = Ui.GoTo("settings"));

        _drawer.Add(new BoxView { HeightRequest = 1, Color = Ui.Line, Margin = new Thickness(0, 6, 0, 6) });
        AddItem("ℹ️", L.T("অ্যাপ সম্পর্কে", "About the app"), null, false, () => Ui.OpenModal(_page, new AboutAppPage()));
        AddItem("📤", L.T("শেয়ার", "Share"), null, false, () => _ = InfoLinks.ShareAppAsync());
        AddItem("🐞", L.T("রিপোর্ট / ফিডব্যাক", "Report / Feedback"), null, false, () => Ui.OpenModal(_page, new BugReportPage()));
    }

    public static string ThemeName() => Theme.Mode switch
    {
        Theme.Dark => L.T("ডার্ক", "Dark"),
        Theme.Light => L.T("লাইট", "Light"),
        _ => L.T("ফোনের মতো", "Auto")
    };

    void AddItem(string icon, string text, string? sub, bool current, Action go)
    {
        var col = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        col.Add(new Label { Text = text, FontSize = 16, FontAttributes = current ? FontAttributes.Bold : FontAttributes.None, TextColor = current ? Ui.Primary : Ui.Ink });
        if (!string.IsNullOrEmpty(sub)) col.Add(new Label { Text = sub, FontSize = 12, TextColor = Ui.Muted });

        var g = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(new GridLength(30)), new ColumnDefinition(GridLength.Star) } };
        g.Add(new Label { Text = icon, FontSize = 21, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 0);
        g.Add(col, 1);

        var b = new Border
        {
            Content = g,
            Padding = new Thickness(10, 11),
            StrokeThickness = 0,
            BackgroundColor = current ? Ui.PrimarySoft : Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await CloseAsync();
                try { go(); } catch (Exception ex) { AppLog.Error("Chrome.Item", ex); }
            })
        });
        _drawer.Add(b);
    }

    // open / close

    void SetWidth()
    {
        double sw = _root.Width > 0 ? _root.Width : 400;
        _w = Math.Clamp(sw * 0.5, 210, sw * 0.8);
        _drawerHost.WidthRequest = _w;
    }

    void Blur(bool on)
    {
        try
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(31)) return;
            if (_push.Handler?.PlatformView is not global::Android.Views.View v) return;
            v.SetRenderEffect(on
                ? global::Android.Graphics.RenderEffect.CreateBlurEffect(16f, 16f, global::Android.Graphics.Shader.TileMode.Clamp)
                : null);
        }
        catch (Exception ex) { AppLog.Error("Chrome.Blur", ex); }
    }

    static bool CanBlur => OperatingSystem.IsAndroidVersionAtLeast(31);

    public async Task OpenAsync()
    {
        if (IsOpen || _anim) return;
        _anim = true;
        try
        {
            MeasureInset();
            SetWidth();
            BuildDrawer();
            _drawerHost.IsVisible = true;
            _scrim.IsVisible = true;
            IsOpen = true;
            Blur(true);
            await Task.WhenAll(
                _push.TranslateTo(_w, 0, 230, Easing.CubicOut),
                _scrim.FadeTo(CanBlur ? 0.18 : 0.4, 230));
        }
        catch (Exception ex) { AppLog.Error("Chrome.Open", ex); }
        finally { _anim = false; }
    }

    public async Task CloseAsync()
    {
        if (!IsOpen || _anim) return;
        _anim = true;
        try
        {
            await Task.WhenAll(
                _push.TranslateTo(0, 0, 200, Easing.CubicIn),
                _scrim.FadeTo(0, 200));
        }
        catch (Exception ex) { AppLog.Error("Chrome.Close", ex); }
        finally
        {
            Reset();
            _anim = false;
        }
    }

    // Instant close (page
    void ForceClose()
    {
        if (!IsOpen) return;
        try { _push.AbortAnimation("TranslateTo"); _scrim.AbortAnimation("FadeTo"); } catch { }
        Reset();
        _anim = false;
    }

    void Reset()
    {
        IsOpen = false;
        _push.TranslationX = 0;
        _scrim.Opacity = 0;
        _scrim.IsVisible = false;
        _drawerHost.IsVisible = false;
        Blur(false);
    }
}
