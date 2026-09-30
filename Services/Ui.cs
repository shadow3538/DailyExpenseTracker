using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using DailyExpenseTracker.Models;

namespace DailyExpenseTracker;

public class RingDrawable : IDrawable
{
    public double Frac { get; set; }
    public Color Fill { get; set; } = Colors.Teal;
    public Color Track { get; set; } = Colors.LightGray;
    public float Thickness { get; set; } = 14;

    public void Draw(ICanvas canvas, RectF r)
    {
        float t = Thickness;
        float cx = r.Width / 2, cy = r.Height / 2;
        float rad = Math.Min(r.Width, r.Height) / 2 - t / 2 - 1;
        if (rad <= 0) return;

        canvas.StrokeSize = t;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.StrokeColor = Track;
        canvas.DrawCircle(cx, cy, rad);

        double f = double.IsNaN(Frac) ? 0 : Math.Clamp(Frac, 0, 1);
        if (f <= 0.002) return;
        canvas.StrokeColor = Fill;
        if (f >= 0.998) { canvas.DrawCircle(cx, cy, rad); return; }

        var p = new PathF();
        int steps = Math.Max(4, (int)(f * 120));
        for (int i = 0; i <= steps; i++)
        {
            double a = 2 * Math.PI * f * i / steps;
            float x = cx + rad * (float)Math.Sin(a);
            float y = cy - rad * (float)Math.Cos(a);
            if (i == 0) p.MoveTo(x, y); else p.LineTo(x, y);
        }
        canvas.DrawPath(p);
    }
}

public static class Ui
{

    static bool D => Theme.IsDark;
    static Color C(string light, string dark) => Color.FromArgb(D ? dark : light);

    public static Color Primary => C("#00897B", "#12A594");
    public static Color PrimaryLight => Color.FromArgb("#80CBC4");
    public static Color PrimarySoft => C("#E0F2F1", "#153634");

    public static Color PrimaryOn => C("#00695C", "#7FDCCF");
    public static Color Ink => C("#1B1F23", "#E8EDF0");
    public static Color Muted => C("#6B7785", "#98A6B2");
    public static Color Line => C("#E3E9EC", "#2C3A44");
    public static Color Green => C("#1E8449", "#4CC38A");
    public static Color GreenSoft => C("#E6F4EA", "#173626");
    public static Color Orange => C("#E67E22", "#F0A04B");
    public static Color Red => C("#C0392B", "#F0776B");
    public static Color RedSoft => C("#FCE8E6", "#3F2220");
    public static Color Gray => C("#EEF1F3", "#25313A");

    public static Color PageBg => C("#F3F6F6", "#0E1418");
    public static Color Surface => C("#FFFFFF", "#182229");
    public static Color FieldBg => C("#F7FAFA", "#202D35");
    public static Color Sep => C("#DDE3E8", "#2C3A44");

    public static Color NavBar => C("#00897B", "#0F3F3A");

    public static readonly Color PrimaryDark = Color.FromArgb("#00695C");
    public static readonly Color PrimaryBright = Color.FromArgb("#00A896");

    public static readonly Color RangeColor = Color.FromArgb("#5C4DB1");
    public static readonly Color RangeDark = Color.FromArgb("#3F3480");
    public static Color RangeSoft => C("#ECE9F9", "#2A2650");

    public static Color LimitColor(double frac) => frac >= 1 ? Red : frac >= 0.75 ? Orange : Green;

    static readonly string[] Palette =
    {
        "#00897B", "#F59E0B", "#3B82F6", "#8B5CF6", "#EC4899", "#EF4444", "#14B8A6", "#84CC16"
    };

    static Dictionary<string, string> _catColors = new();
    static Dictionary<string, string> _catIcons = new();

    public static void SetCategoryStyles(IEnumerable<ExpenseCategory> cats)
    {
        var colors = new Dictionary<string, string>();
        var icons = new Dictionary<string, string>();
        foreach (var c in cats)
        {
            colors[c.Name] = c.Color;
            icons[c.Name] = c.Icon;
        }
        _catColors = colors;
        _catIcons = icons;
    }

    public static Color ItemColor(string name)
    {
        if (name != null && _catColors.TryGetValue(name, out var hex))
        {
            try { return Color.FromArgb(hex); } catch { }
        }
        int h = 0;
        foreach (var ch in name ?? "") h = (h * 31 + ch) & 0x7fffffff;
        return Color.FromArgb(Palette[h % Palette.Length]);
    }

    static string? KeywordIcon(string name)
    {
        var n = (name ?? "").ToLowerInvariant();
        if (n.Contains("সকাল")) return "🍳";
        if (n.Contains("দুপুর")) return "🍛";
        if (n.Contains("রাত")) return "🍽️";
        if (n.Contains("নাস্তা")) return "🥪";
        if (n.Contains("extra")) return "✨";
        if (n.Contains("সিএনজি") || n.Contains("রিক্সা") || n.Contains("রিকশা")) return "🛺";
        if (n.Contains("ট্রেন")) return "🚆";
        if (n.Contains("মেট্রো")) return "🚇";
        if (n.Contains("বাস") || n.Contains("ভাড়া") || n.Contains("যাতায়াত") || n.Contains("গাড়ি")) return "🚌";
        if (n.Contains("ঔষধ") || n.Contains("ওষুধ")) return "💊";
        if (n.Contains("বাজার")) return "🛒";

        if (n.Contains("breakfast")) return "🍳";
        if (n.Contains("lunch")) return "🍛";
        if (n.Contains("dinner")) return "🍽️";
        if (n.Contains("snack")) return "🥪";
        if (n.Contains("cng") || n.Contains("rickshaw")) return "🛺";
        if (n.Contains("train")) return "🚆";
        if (n.Contains("metro")) return "🚇";
        if (n.Contains("bus") || n.Contains("fare") || n.Contains("transport") || n.Contains("car")) return "🚌";
        if (n.Contains("medicine")) return "💊";
        if (n.Contains("grocer")) return "🛒";
        return null;
    }

    public static string ItemIcon(string name)
    {
        if (name != null && _catIcons.TryGetValue(name, out var ic)) return ic;
        return KeywordIcon(name ?? "") ?? "🧾";
    }

    public static string EntryIcon(Expense e)
    {
        var kw = KeywordIcon(e.Item ?? "");
        if (kw != null && !string.Equals(e.Item, e.Cat, StringComparison.Ordinal)) return kw;
        return ItemIcon(e.Cat);
    }

    public static View ItemBadge(string name, double size = 44) => Badge(ItemColor(name), ItemIcon(name), size);

    public static View Badge(Color c, string icon, double size = 44)
    {
        return new Border
        {
            WidthRequest = size,
            HeightRequest = size,
            StrokeThickness = 0,
            BackgroundColor = c.WithAlpha(0.16f),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(size / 2) },
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = icon,
                FontSize = size * 0.46,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
    }

    public static View Chip(string text, Color bg, Color fg, double size = 12)
    {
        return new Border
        {
            Padding = new Thickness(10, 4),
            StrokeThickness = 0,
            BackgroundColor = bg,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = new Label { Text = text, FontSize = size, TextColor = fg, FontAttributes = FontAttributes.Bold }
        };
    }

    public static View Avatar(double size = 44, bool ring = false) =>
        AvatarFrom(Profile.PhotoPath, Profile.Name, size, ring);

    public static View AvatarFrom(string? photoPath, string? name, double size = 44, bool ring = false)
    {
        View content;
        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            var path = photoPath;
            content = new Image
            {
                Aspect = Aspect.AspectFill,
                Source = ImageSource.FromStream(() => File.OpenRead(path))
            };
        }
        else
        {
            var initial = Profile.InitialOf(name);
            content = new Label
            {
                Text = initial.Length > 0 ? initial : "👤",
                FontSize = size * (initial.Length > 0 ? 0.42 : 0.46),
                FontAttributes = FontAttributes.Bold,
                TextColor = Primary,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        return new Border
        {
            WidthRequest = size,
            HeightRequest = size,
            StrokeThickness = ring ? 3 : 0,
            Stroke = Surface,
            BackgroundColor = PrimarySoft,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(size / 2) },
            Content = content
        };
    }

    public static View Bar(double frac, Color fill, Color? track = null, double height = 10)
    {
        if (double.IsNaN(frac) || frac < 0) frac = 0;
        if (frac > 1) frac = 1;

        var g = new Grid { HeightRequest = height, ColumnSpacing = 0, RowSpacing = 0 };
        g.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(frac, GridUnitType.Star)));
        g.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - frac, GridUnitType.Star)));

        var back = new BoxView { Color = track ?? Line, CornerRadius = height / 2 };
        Grid.SetColumnSpan(back, 2);
        g.Add(back);
        if (frac > 0)
            g.Add(new BoxView { Color = fill, CornerRadius = height / 2 }, 0);
        return g;
    }

    public static GraphicsView Ring(double frac, Color fill, double size = 120, float thickness = 14)
    {
        return new GraphicsView
        {
            WidthRequest = size,
            HeightRequest = size,
            Drawable = new RingDrawable { Frac = frac, Fill = fill, Track = Line, Thickness = thickness }
        };
    }

    public static View EntryRow(Expense e, Action<Expense> onTap, bool showDate = false)
    {
        var mid = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        mid.Add(new Label { Text = string.IsNullOrWhiteSpace(e.Item) ? "Extra" : e.Item, FontSize = 16, FontAttributes = FontAttributes.Bold });

        if (!string.IsNullOrWhiteSpace(e.Category) && !string.Equals(e.Item, e.Category, StringComparison.Ordinal))
            mid.Add(new Label { Text = e.Category, FontSize = 12, TextColor = ItemColor(e.Category), FontAttributes = FontAttributes.Bold });

        if (!string.IsNullOrWhiteSpace(e.Note))
            mid.Add(new Label { Text = e.Note, FontSize = 13, TextColor = Ink, MaxLines = 3, LineBreakMode = LineBreakMode.TailTruncation });

        mid.Add(new Label
        {
            Text = e.Date.ToString(showDate ? "dd MMM, hh:mm tt" : "hh:mm tt", Fmt.Inv),
            FontSize = 12,
            TextColor = Muted
        });

        var right = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        right.Add(new Label { Text = Fmt.Money0(e.Amount), FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Primary, HorizontalTextAlignment = TextAlignment.End });
        right.Add(new Label { Text = L.T("✎ এডিট", "✎ Edit"), FontSize = 11, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End });

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
        g.Add(Badge(ItemColor(e.Cat), EntryIcon(e), 44), 0);
        g.Add(mid, 1);
        g.Add(right, 2);

        var b = new Border
        {
            Content = g,
            Padding = new Thickness(12, 10),
            BackgroundColor = Surface,
            Stroke = Line,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => onTap(e)) });
        return b;
    }

    public static async Task AnimateInAsync(View view, uint duration = 220, int delay = 0)
    {
        if (delay > 0) await Task.Delay(delay);
        view.Opacity = 0;
        view.TranslationY = 10;
        await Task.WhenAll(
            view.FadeTo(1, duration, Easing.CubicOut),
            view.TranslateTo(0, 0, duration, Easing.CubicOut));
    }

    public static async Task TapPulseAsync(VisualElement view)
    {
        try
        {
            await view.ScaleTo(0.96, 55, Easing.CubicOut);
            await view.ScaleTo(1, 85, Easing.CubicInOut);
        }
        catch (Exception ex) { AppLog.Error("TapPulse", ex); }
    }

    public static void OpenEdit(Page from, Expense e)
    {
        var nav = new NavigationPage(new EditPage(e))
        {
            BarBackgroundColor = NavBar,
            BarTextColor = Colors.White
        };
        _ = from.Navigation.PushModalAsync(nav);
    }

    public static void OpenDay(Page from, DateTime day)
    {
        var nav = new NavigationPage(new DayDetailPage(day))
        {
            BarBackgroundColor = NavBar,
            BarTextColor = Colors.White
        };
        _ = from.Navigation.PushModalAsync(nav);
    }

    public static void OpenModal(Page from, Page page)
    {
        var nav = new NavigationPage(page)
        {
            BarBackgroundColor = NavBar,
            BarTextColor = Colors.White
        };
        _ = from.Navigation.PushModalAsync(nav);
    }

    public static void OpenProfile(Page from)
    {
        var nav = new NavigationPage(new ProfilePage())
        {
            BarBackgroundColor = NavBar,
            BarTextColor = Colors.White
        };
        _ = from.Navigation.PushModalAsync(nav);
    }

    public static void OpenProfileEdit(Page from)
    {
        var nav = new NavigationPage(new ProfileEditPage())
        {
            BarBackgroundColor = NavBar,
            BarTextColor = Colors.White
        };
        _ = from.Navigation.PushModalAsync(nav);
    }

    public static DateTime? PendingAddDate;

    public static string? CurrentRoute;
    public static string? PrevRoute;

    public static string? ReturnRoute;

    public static bool PendingOpenRange;

    public static Task GoTo(string route)
    {
        if (route == "history") route = "report";
        return Shell.Current.GoToAsync("//" + route);
    }

    public static Task GoBackToOrigin()
    {
        var r = PrevRoute;
        if (string.IsNullOrEmpty(r) || r == CurrentRoute) r = "home";
        return GoTo(r);
    }

    public static void Toast(string text)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    global::Android.Widget.Toast.MakeText(
                        global::Android.App.Application.Context, text,
                        global::Android.Widget.ToastLength.Long)?.Show();
                }
                catch { }
            });
        }
        catch { }
    }
}
