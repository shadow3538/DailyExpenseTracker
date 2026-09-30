using Microsoft.Maui.Controls.Shapes;

namespace DailyExpenseTracker;

public static class InfoLinks
{
    public static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);

    public static string Normalize(string value, string url)
    {
        var u = (Has(url) ? url : value).Trim();
        if (u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            u.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            u.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            u.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)) return u;
        if (u.Contains('@') && !u.Contains('/')) return "mailto:" + u;
        var digits = u.Replace(" ", "").Replace("-", "");
        if (digits.Length >= 6 && digits.All(c => char.IsDigit(c) || c == '+')) return "tel:" + digits;
        return "https://" + u;
    }

    public static async Task OpenAsync(string uri)
    {
        try { await Launcher.Default.OpenAsync(uri); }
        catch (Exception ex)
        {
            AppLog.Error("Info.Open", ex);
            Ui.Toast(L.T("লিংকটি খোলা যায়নি", "Could not open the link"));
        }
    }

    public static async Task CopyAsync(string text)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(text);
            Ui.Toast(L.T("কপি হয়েছে", "Copied"));
        }
        catch (Exception ex) { AppLog.Error("Info.Copy", ex); }
    }

    public static async Task ShareAppAsync()
    {
        if (!Has(AppInfo.DownloadUrl))
        {
            Ui.Toast(L.T("ডাউনলোড লিংক এখনো যোগ করা হয়নি", "The download link has not been added yet"));
            return;
        }
        try
        {
            var name = L.T(AppInfo.AppNameBn, AppInfo.AppNameEn);
            var text = L.T(
                $"{name} — দৈনিক খরচের হিসাব রাখার সহজ অ্যাপ। ডাউনলোড করুন:\n{AppInfo.DownloadUrl}",
                $"{name} — a simple app to track your daily expenses. Download:\n{AppInfo.DownloadUrl}");
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = L.T("অ্যাপ শেয়ার করুন", "Share the app"),
                Subject = name,
                Text = text
            });
        }
        catch (Exception ex) { AppLog.Error("Info.Share", ex); }
    }

    public static View Row(string icon, string title, string? sub, Action? onTap = null, bool chevron = false)
    {
        var col = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        col.Add(new Label { Text = title, FontSize = 15, FontAttributes = FontAttributes.Bold });
        if (Has(sub)) col.Add(new Label { Text = sub, FontSize = 13, TextColor = Ui.Muted });

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
        g.Add(Ui.Badge(Ui.Primary, icon, 40), 0);
        g.Add(col, 1);
        if (chevron)
            g.Add(new Label { Text = "›", FontSize = 24, TextColor = Ui.Muted, VerticalOptions = LayoutOptions.Center }, 2);
        if (onTap != null)
            g.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => onTap()) });
        return g;
    }
}

public class AboutAppPage : ModalBase
{
    public AboutAppPage() : base(L.T("অ্যাপ সম্পর্কে", "About the app"))
    {
        var root = NewRoot();

        var head = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 4, 0, 6) };
        head.Add(new Border
        {
            WidthRequest = 84,
            HeightRequest = 84,
            StrokeThickness = 0,
            BackgroundColor = Ui.PrimarySoft,
            HorizontalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(26) },
            Content = new Label { Text = "💰", FontSize = 42, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        head.Add(new Label
        {
            Text = L.T(AppInfo.AppNameBn, AppInfo.AppNameEn),
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        });
        head.Add(new Label { Text = "v" + AppInfo.Version, FontSize = 13, TextColor = Ui.Muted, HorizontalTextAlignment = TextAlignment.Center });
        root.Add(head);

        root.Add(new Label
        {
            Text = L.T(
                "প্রতিদিনের খরচ দ্রুত লিখে রাখা আর হিসাব বুঝে চলার সহজ অ্যাপ। ইন্টারনেট ছাড়াই চলে, ডেটা থাকে আপনার ফোনেই।",
                "A simple app to log daily spending quickly and stay on top of your budget. Works offline; your data stays on your phone."),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Ui.Muted
        });

        root.Add(new Label { Text = L.T("✨ ফিচার", "✨ Features"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 8, 0, 0) });
        var feats = new VerticalStackLayout { Spacing = 14 };
        foreach (var f in AppInfo.Features)
        {
            var text = L.T(f.TextBn, f.TextEn);
            if (!InfoLinks.Has(text)) continue;
            feats.Add(InfoLinks.Row(f.Icon, L.T(f.TitleBn, f.TitleEn), text));
        }
        root.Add(FUi.Card(feats, null, 14));

        root.Add(new Label { Text = L.T("⬇️ ডাউনলোড", "⬇️ Download"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 8, 0, 0) });
        var dl = new VerticalStackLayout { Spacing = 10 };
        if (InfoLinks.Has(AppInfo.DownloadUrl))
        {
            dl.Add(new Label { Text = L.T("অ্যাপটি এখান থেকে ডাউনলোড করা যাবে:", "You can download the app from here:"), FontSize = 13, TextColor = Ui.Muted });
            dl.Add(new Label
            {
                Text = AppInfo.DownloadUrl,
                FontSize = 14,
                TextColor = Ui.Primary,
                TextDecorations = TextDecorations.Underline,
                LineBreakMode = LineBreakMode.CharacterWrap
            });
            var row = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            row.Add(FUi.Btn(L.T("🌐 খুলুন", "🌐 Open"), Ui.Primary, true, () => _ = InfoLinks.OpenAsync(AppInfo.DownloadUrl), 44), 0);
            row.Add(FUi.Btn(L.T("📋 কপি", "📋 Copy"), Ui.Primary, false, () => _ = InfoLinks.CopyAsync(AppInfo.DownloadUrl), 44), 1);
            dl.Add(row);
        }
        else
        {
            dl.Add(new Label
            {
                Text = L.T("ডাউনলোড লিংক শীঘ্রই যোগ করা হবে।", "The download link will be added soon."),
                FontSize = 14,
                TextColor = Ui.Muted
            });
        }
        root.Add(FUi.Card(dl, null, 14));

        if (InfoLinks.Has(AppInfo.GitHubUrl))
        {
            root.Add(new Label { Text = L.T("🔄 আপডেট", "🔄 Updates"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 8, 0, 0) });
            var up = new VerticalStackLayout { Spacing = 10 };
            var status = new Label { FontSize = 14 };
            up.Add(status);
            var btn = FUi.Btn(L.T("🔍 এখনই চেক করুন", "🔍 Check now"), Ui.Primary, false, () => { }, 44);
            var open = FUi.Btn(L.T("⬇️ নতুন ভার্সন ডাউনলোড", "⬇️ Download the new version"), Ui.Primary, true, () => { }, 44);
            open.IsVisible = false;
            string openUrl = "";

            void Show(bool known, string latest)
            {
                if (known)
                {
                    status.Text = L.T($"নতুন ভার্সন {latest} পাওয়া যাচ্ছে (আপনার ভার্সন {AppInfo.Version})।", $"Version {latest} is available (you have {AppInfo.Version}).");
                    status.TextColor = Ui.Orange;
                    openUrl = InfoLinks.Has(UpdateChecker.LatestUrl) ? UpdateChecker.LatestUrl : AppInfo.DownloadUrl;
                    open.IsVisible = true;
                }
                else
                {
                    status.Text = L.T($"আপনি সর্বশেষ ভার্সন (v{AppInfo.Version}) ব্যবহার করছেন।", $"You are on the latest version (v{AppInfo.Version}).");
                    status.TextColor = Ui.Muted;
                    open.IsVisible = false;
                }
            }
            Show(UpdateChecker.UpdateKnown, UpdateChecker.LatestSeen);
            open.Clicked += (_, _) => _ = InfoLinks.OpenAsync(openUrl);
            btn.Clicked += async (_, _) =>
            {
                btn.IsEnabled = false;
                status.Text = L.T("চেক করা হচ্ছে…", "Checking…");
                status.TextColor = Ui.Muted;
                var r = await UpdateChecker.CheckAsync(notify: false);
                if (!r.Ok)
                {
                    status.Text = L.T("চেক করা যায়নি। ইন্টারনেট আছে কি না দেখুন।", "Could not check. Please check your internet connection.");
                    status.TextColor = Ui.Red;
                }
                else Show(r.HasUpdate, r.Latest);
                btn.IsEnabled = true;
            };
            up.Add(btn);
            up.Add(open);
            root.Add(FUi.Card(up, null, 14));
        }

        root.Add(new Label { Text = L.T("👨‍💻 ডেভেলপার", "👨‍💻 Developer"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 8, 0, 0) });
        var dev = new VerticalStackLayout { Spacing = 10 };
        foreach (var d in AppInfo.Developers)
        {
            if (!InfoLinks.Has(d.Name)) continue;
            dev.Add(InfoLinks.Row(InfoLinks.Has(d.Icon) ? d.Icon : "👤", d.Name, L.T(d.NoteBn, d.NoteEn)));
        }
        root.Add(FUi.Card(dev, null, 14));

        Content = new ScrollView { Content = root };
    }
}

public class DeveloperPage : ModalBase
{
    public DeveloperPage() : base(L.T("ডেভেলপার সম্পর্কে", "About the developer"))
    {
        var root = NewRoot();

        foreach (var d in AppInfo.Developers)
        {
            if (!InfoLinks.Has(d.Name)) continue;
            var h = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 4, 0, 4) };
            h.Add(new Border
            {
                WidthRequest = 84,
                HeightRequest = 84,
                StrokeThickness = 0,
                BackgroundColor = Ui.PrimarySoft,
                HorizontalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(42) },
                Content = new Label
                {
                    Text = InfoLinks.Has(d.Icon) ? d.Icon : "👤",
                    FontSize = 40,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
            h.Add(new Label { Text = d.Name, FontSize = 22, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
            var note = L.T(d.NoteBn, d.NoteEn);
            if (InfoLinks.Has(note))
                h.Add(new Label { Text = note, FontSize = 13, TextColor = Ui.Muted, HorizontalTextAlignment = TextAlignment.Center });
            root.Add(h);
        }

        var bio = L.T(AppInfo.DevBioBn, AppInfo.DevBioEn);
        if (InfoLinks.Has(bio))
            root.Add(FUi.Card(new Label { Text = bio, FontSize = 14 }, null, 14));

        var extra = new VerticalStackLayout { Spacing = 14 };
        int extraCount = 0;
        foreach (var x in AppInfo.DevExtra)
        {
            var text = L.T(x.TextBn, x.TextEn);
            if (!InfoLinks.Has(text)) continue;
            extra.Add(InfoLinks.Row(x.Icon, L.T(x.TitleBn, x.TitleEn), text));
            extraCount++;
        }
        if (extraCount > 0) root.Add(FUi.Card(extra, null, 14));

        var contacts = new VerticalStackLayout { Spacing = 14 };
        int contactCount = 0;
        foreach (var c in AppInfo.DevContacts)
        {
            if (!InfoLinks.Has(c.Value)) continue;
            var uri = InfoLinks.Normalize(c.Value, c.Url);
            contacts.Add(InfoLinks.Row(c.Icon, L.T(c.LabelBn, c.LabelEn), c.Value, () => _ = InfoLinks.OpenAsync(uri), true));
            contactCount++;
        }
        if (contactCount > 0)
        {
            root.Add(new Label { Text = L.T("📬 যোগাযোগ", "📬 Contact"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 6, 0, 0) });
            root.Add(FUi.Card(contacts, null, 14));
        }

        root.Add(new Label { Text = L.T("📦 অ্যাপ", "📦 App"), StyleClass = new[] { "H2" }, Margin = new Thickness(0, 6, 0, 0) });
        var app = new VerticalStackLayout { Spacing = 14 };
        app.Add(InfoLinks.Row("💰", L.T(AppInfo.AppNameBn, AppInfo.AppNameEn), "v" + AppInfo.Version));
        if (InfoLinks.Has(AppInfo.DownloadUrl))
            app.Add(InfoLinks.Row("⬇️", L.T("ডাউনলোড লিংক", "Download link"), AppInfo.DownloadUrl,
                () => _ = InfoLinks.OpenAsync(AppInfo.DownloadUrl), true));
        root.Add(FUi.Card(app, null, 14));

        Content = new ScrollView { Content = root };
    }
}

public class BugReportPage : ModalBase
{
    readonly Editor _msg = new()
    {
        AutoSize = EditorAutoSizeOption.TextChanges,
        MinimumHeightRequest = 150,
        FontSize = 15,
        BackgroundColor = Colors.Transparent
    };

    readonly List<(string Key, Border Chip, Label Text)> _chips = new();
    string _kind = "bug";

    public BugReportPage() : base(L.T("রিপোর্ট / ফিডব্যাক", "Report / Feedback"))
    {
        _msg.Placeholder = L.T("কী সমস্যা হয়েছে বা কী বলতে চান লিখুন…", "Describe the problem or what you'd like to say…");

        var root = NewRoot();
        root.Add(FUi.Muted(L.T(
            "কোনো বাগ পেলে বা মতামত থাকলে এখানে জানান। কী করার সময় সমস্যা হলো তা লিখলে ঠিক করতে সুবিধা হয়।",
            "Found a bug or have an idea? Tell us here. Describing what you were doing helps us fix it."), 14));

        var kinds = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        AddChip(kinds, 0, "bug", "🐞", L.T("বাগ", "Bug"));
        AddChip(kinds, 1, "feedback", "💬", L.T("ফিডব্যাক", "Feedback"));
        AddChip(kinds, 2, "idea", "💡", L.T("পরামর্শ", "Idea"));
        root.Add(kinds);
        UpdateChips();

        root.Add(FUi.Field(_msg));

        root.Add(FUi.Muted(L.T(
            "সাথে অ্যাপ ভার্সন, ফোনের মডেল ও অ্যান্ড্রয়েড ভার্সন যুক্ত হবে। আপনার খরচের কোনো ডেটা পাঠানো হয় না।",
            "App version, phone model and Android version are attached. None of your expense data is sent."), 12));

        root.Add(FUi.Btn(L.T("📤 পাঠান", "📤 Send"), Ui.Primary, true, () => _ = SendAsync(), 50));

        if (InfoLinks.Has(AppInfo.GitHubUrl))
        {
            root.Add(FUi.Btn(L.T("🐙 গিটহাবে ইস্যু খুলুন", "🐙 Open a GitHub issue"), Ui.Primary, false,
                () => _ = InfoLinks.OpenAsync(AppInfo.GitHubUrl.TrimEnd('/') + "/issues/new"), 46));
        }

        Content = new ScrollView { Content = root };
    }

    void AddChip(Grid host, int col, string key, string icon, string title)
    {
        var lbl = new Label
        {
            Text = icon + " " + title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        var b = new Border
        {
            HeightRequest = 44,
            Padding = new Thickness(6, 0),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Content = lbl
        };
        b.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => { _kind = key; UpdateChips(); })
        });
        _chips.Add((key, b, lbl));
        host.Add(b, col, 0);
    }

    void UpdateChips()
    {
        foreach (var (key, chip, text) in _chips)
        {
            bool on = key == _kind;
            chip.BackgroundColor = on ? Ui.PrimarySoft : Ui.Surface;
            chip.Stroke = on ? Ui.Primary : Ui.Line;
            text.TextColor = on ? Ui.Primary : Ui.Ink;
        }
    }

    string KindLabel() => _kind switch
    {
        "bug" => "Bug",
        "idea" => "Idea",
        _ => "Feedback"
    };

    string BuildBody(string message)
    {
        string device;
        try { device = $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}"; } catch { device = "?"; }
        string os;
        try { os = $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}"; } catch { os = "?"; }

        return message.Trim() +
               "\n\n----------\n" +
               $"App: {AppInfo.AppNameEn} v{AppInfo.Version}\n" +
               $"Device: {device}\n" +
               $"OS: {os}\n" +
               $"Language: {(L.IsEn ? "English" : "Bangla")}";
    }

    async Task SendAsync()
    {
        var message = _msg.Text?.Trim() ?? "";
        if (message.Length == 0)
        {
            Ui.Toast(L.T("আগে কিছু লিখুন", "Please write something first"));
            return;
        }

        var subject = $"[{AppInfo.AppNameEn}] {KindLabel()}";
        var body = BuildBody(message);

        try
        {
            if (InfoLinks.Has(AppInfo.ReportEmail))
            {
                try
                {
                    await Email.Default.ComposeAsync(new EmailMessage
                    {
                        Subject = subject,
                        Body = body,
                        To = new List<string> { AppInfo.ReportEmail }
                    });
                    return;
                }
                catch (Exception ex)
                {

                    AppLog.Error("Report.Email", ex);
                }
            }

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = L.T("রিপোর্ট পাঠান", "Send report"),
                Subject = subject,
                Text = subject + "\n\n" + body
            });
        }
        catch (Exception ex)
        {
            AppLog.Error("Report.Send", ex);
            Ui.Toast(L.T("পাঠানো যায়নি", "Could not send"));
        }
    }
}
