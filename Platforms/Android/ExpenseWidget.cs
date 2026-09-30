using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using APaint = Android.Graphics.Paint;
using ARectF = Android.Graphics.RectF;
using ACanvas = Android.Graphics.Canvas;

namespace DailyExpenseTracker;

// Home screen widget
[BroadcastReceiver(Label = "পকেটনামা", Exported = true)]
[IntentFilter(new[]
{
    "android.appwidget.action.APPWIDGET_UPDATE",
    "android.intent.action.DATE_CHANGED",      // midnight day change
    "android.intent.action.TIME_SET",
    "android.intent.action.TIMEZONE_CHANGED"
})]
[MetaData("android.appwidget.provider", Resource = "@xml/expense_widget_info")]
public class ExpenseWidget : AppWidgetProvider
{
    // Size thresholds (dp)
    const int SmallMaxWidthDp = 190;   // narrower = small layout
    const int LargeMinHeightDp = 175;  // taller = large layout

    static readonly AColor OverText = AColor.ParseColor("#FFB4A8");   // light red text
    static readonly AColor OverFill = AColor.ParseColor("#FF8A80");
    static readonly AColor WarnFill = AColor.ParseColor("#FFD180");

    // system callbacks

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null) return;
        var a = intent?.Action;
        if (a == "android.intent.action.DATE_CHANGED" ||
            a == "android.intent.action.TIME_SET" ||
            a == "android.intent.action.TIMEZONE_CHANGED")
        {
            Run(context, null, GoAsync());
            return;
        }
        base.OnReceive(context, intent);
    }

    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context == null) return;
        Run(context, appWidgetIds, GoAsync());
    }

    // Widget resized
    public override void OnAppWidgetOptionsChanged(Context? context, AppWidgetManager? appWidgetManager, int appWidgetId, Bundle? newOptions)
    {
        base.OnAppWidgetOptionsChanged(context, appWidgetManager, appWidgetId, newOptions);
        if (context == null) return;
        Run(context, new[] { appWidgetId }, GoAsync());
    }

    // Refresh all widgets
    public static void Refresh(Context ctx) => Run(ctx, null, null);

    // update

    static void Run(Context ctx, int[]? ids, BroadcastReceiver.PendingResult? pending)
    {
        var app = ctx.ApplicationContext ?? ctx;
        Task.Run(async () =>
        {
            try
            {
                var mgr = AppWidgetManager.GetInstance(app);
                if (mgr == null) return;
                ids ??= mgr.GetAppWidgetIds(new ComponentName(app, Java.Lang.Class.FromType(typeof(ExpenseWidget))));
                if (ids == null || ids.Length == 0) return;

                var data = await WidgetData.LoadAsync();
                foreach (var id in ids)
                {
                    try { mgr.UpdateAppWidget(id, Build(app, mgr, id, data)); }
                    catch { }
                }
            }
            catch { }
            finally
            {
                try { pending?.Finish(); } catch { }
            }
        });
    }

    enum Kind { Small, Medium, Large }

    static RemoteViews Build(Context ctx, AppWidgetManager mgr, int id, WidgetData d)
    {
        var opt = mgr.GetAppWidgetOptions(id);
        int w = opt?.GetInt(AppWidgetManager.OptionAppwidgetMinWidth, 250) ?? 250;
        int h = opt?.GetInt(AppWidgetManager.OptionAppwidgetMinHeight, 110) ?? 110;
        float den = ctx.Resources?.DisplayMetrics?.Density ?? 3f;

        var kind = w < SmallMaxWidthDp ? Kind.Small : h >= LargeMinHeightDp ? Kind.Large : Kind.Medium;
        var today = TodayCard(d);

        RemoteViews rv;
        switch (kind)
        {
            case Kind.Small:
                rv = new RemoteViews(ctx.PackageName!, Resource.Layout.widget_small);
                Bind(rv, today, Resource.Id.today_label, Resource.Id.today_amount, Resource.Id.today_info, Resource.Id.today_bar, w - 24, 7, den);
                break;

            case Kind.Medium:
            {
                rv = new RemoteViews(ctx.PackageName!, Resource.Layout.widget_medium);
                float bw = (w - 16 - 6) / 2f - 16;
                Bind(rv, today, Resource.Id.today_label, Resource.Id.today_amount, Resource.Id.today_info, Resource.Id.today_bar, bw, 6, den);
                Bind(rv, MonthCard(d), Resource.Id.month_label, Resource.Id.month_amount, Resource.Id.month_info, Resource.Id.month_bar, bw, 6, den);
                rv.SetOnClickPendingIntent(Resource.Id.card_month, Open(ctx, "report", 3));
                break;
            }

            default:
            {
                rv = new RemoteViews(ctx.PackageName!, Resource.Layout.widget_large);
                float bw = (w - 20 - 6) / 2f - 16;
                Bind(rv, today, Resource.Id.today_label, Resource.Id.today_amount, Resource.Id.today_info, Resource.Id.today_bar, bw, 6, den);
                Bind(rv, MonthCard(d), Resource.Id.month_label, Resource.Id.month_amount, Resource.Id.month_info, Resource.Id.month_bar, bw, 6, den);
                try { rv.SetTextViewText(Resource.Id.btn_add, L.T("+ খরচ যোগ", "+ Add")); } catch { }
                rv.SetTextViewText(Resource.Id.header_title, L.T("পকেটনামা · ") + Fmt.BnWeekday(d.Today) + ", " + Fmt.DayMonth(d.Today));

                // Chart gets height
                float chartH = Math.Clamp(h - 137, 40, 150);
                rv.SetImageViewBitmap(Resource.Id.chart, ChartBitmap(den, w - 20, chartH, d));
                rv.SetOnClickPendingIntent(Resource.Id.chart, Open(ctx, "report", 4));
                rv.SetOnClickPendingIntent(Resource.Id.card_month, Open(ctx, "report", 3));
                break;
            }
        }

        rv.SetOnClickPendingIntent(Resource.Id.widget_root, Open(ctx, "home", 1));
        rv.SetOnClickPendingIntent(Resource.Id.btn_add, Open(ctx, "add", 2));
        if (kind != Kind.Small) rv.SetOnClickPendingIntent(Resource.Id.card_today, Open(ctx, "home", 1));
        return rv;
    }

    // Tap opens app
    static PendingIntent Open(Context ctx, string route, int code)
    {
        var i = new Intent(ctx, typeof(MainActivity));
        i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        i.PutExtra("route", route);
        return PendingIntent.GetActivity(ctx, code, i, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    // card texts

    class Card
    {
        public string Label = "", Amount = "", Info = "";
        public double Frac;
        public bool HasBar, Over;
    }

    static string Pct(double frac) => Math.Round(frac * 100).ToString(Fmt.Inv) + "%";

    static Card TodayCard(WidgetData d)
    {
        var c = new Card();
        if (d.TodayLimit <= 0)
        {
            c.Label = L.T("আজকের খরচ");
            c.Amount = Fmt.Money0(d.TodaySpent);
            c.Info = L.T("দৈনিক লিমিট নেই");
            return c;
        }
        c.Frac = (double)(d.TodaySpent / d.TodayLimit);
        c.HasBar = true;
        var left = d.TodayLimit - d.TodaySpent;
        if (left >= 0) { c.Label = L.T("আজ বাকি"); c.Amount = Fmt.Money0(left); }
        else { c.Label = L.T("লিমিট ছাড়িয়েছে"); c.Amount = Fmt.Money0(-left); c.Over = true; }
        c.Info = L.T("লিমিট ") + Fmt.Money0(d.TodayLimit) + " · " + Pct(c.Frac);
        return c;
    }

    static Card MonthCard(WidgetData d)
    {
        var what = d.Custom ? L.T("রেঞ্জে") : L.T("মাসে");
        var c = new Card();
        if (d.PeriodLimit <= 0)
        {
            c.Label = what + L.T(" খরচ");
            c.Amount = Fmt.Money0(d.PeriodSpent);
            c.Info = L.T("লিমিট সেট নেই");
            return c;
        }
        c.Frac = (double)(d.PeriodSpent / d.PeriodLimit);
        c.HasBar = true;
        var left = d.PeriodLimit - d.PeriodSpent;
        if (left >= 0) { c.Label = what + L.T(" বাকি"); c.Amount = Fmt.Money0(left); }
        else { c.Label = L.T("লিমিট ছাড়িয়েছে"); c.Amount = Fmt.Money0(-left); c.Over = true; }
        c.Info = L.T("খরচ ") + Fmt.Money0(d.PeriodSpent) + " · " + Pct(c.Frac);
        return c;
    }

    static void Bind(RemoteViews rv, Card c, int label, int amount, int info, int bar, float barWidthDp, float barHeightDp, float den)
    {
        rv.SetTextViewText(label, c.Label);
        rv.SetTextViewText(amount, c.Amount);
        rv.SetTextColor(amount, c.Over ? OverText : AColor.White);
        rv.SetTextViewText(info, c.Info);
        if (c.HasBar)
        {
            rv.SetViewVisibility(bar, ViewStates.Visible);
            rv.SetImageViewBitmap(bar, BarBitmap(den, Math.Max(40, barWidthDp), barHeightDp, c.Frac));
        }
        else rv.SetViewVisibility(bar, ViewStates.Invisible);
    }

    // drawing (bar, chart)

    // Limit bar: orange
    static Bitmap BarBitmap(float den, float wDp, float hDp, double frac)
    {
        int W = Math.Max(1, (int)(wDp * den)), H = Math.Max(1, (int)(hDp * den));
        var bmp = Bitmap.CreateBitmap(W, H, Bitmap.Config.Argb8888!)!;
        var c = new ACanvas(bmp);
        var p = new APaint(PaintFlags.AntiAlias);

        p.Color = AColor.Argb(64, 255, 255, 255);
        c.DrawRoundRect(new ARectF(0, 0, W, H), H / 2f, H / 2f, p);

        var f = Math.Clamp(frac, 0, 1);
        if (f > 0)
        {
            p.Color = frac >= 1 ? OverFill : frac >= 0.75 ? WarnFill : AColor.White;
            float fw = Math.Max(H, (float)(W * f));
            c.DrawRoundRect(new ARectF(0, 0, fw, H), H / 2f, H / 2f, p);
        }
        return bmp;
    }

    // 7-day bar chart
    static Bitmap ChartBitmap(float den, float wDp, float hDp, WidgetData d)
    {
        int W = Math.Max(1, (int)(wDp * den)), H = Math.Max(1, (int)(hDp * den));
        var bmp = Bitmap.CreateBitmap(W, H, Bitmap.Config.Argb8888!)!;
        var c = new ACanvas(bmp);
        var p = new APaint(PaintFlags.AntiAlias);
        int n = d.Week.Count;
        if (n == 0) return bmp;

        float topPad = 12 * den, botPad = 14 * den;
        float areaH = Math.Max(10 * den, H - topPad - botPad);
        float baseY = topPad + areaH;

        decimal max = 1;
        foreach (var x in d.Week) max = Math.Max(max, Math.Max(x.Spent, x.Allowed));

        float slot = W / (float)n;
        float bw = slot * 0.46f;
        float rad = 4 * den;

        // Dotted limit line
        p.SetStyle(APaint.Style.Stroke);
        p.StrokeWidth = 1f * den;
        p.Color = AColor.Argb(120, 255, 255, 255);
        p.SetPathEffect(new DashPathEffect(new[] { 5 * den, 4 * den }, 0));
        for (int i = 0; i < n; i++)
        {
            var a = d.Week[i].Allowed;
            if (a <= 0) continue;
            float y = baseY - (float)(a / max) * areaH;
            c.DrawLine(i * slot + slot * 0.1f, y, (i + 1) * slot - slot * 0.1f, y, p);
        }
        p.SetPathEffect(null);

        p.SetStyle(APaint.Style.Fill);
        p.TextAlign = APaint.Align.Center;
        p.TextSize = 9.5f * den;

        for (int i = 0; i < n; i++)
        {
            var day = d.Week[i];
            bool isToday = day.Day.Date == d.Today.Date;
            bool over = day.Allowed > 0 && day.Spent > day.Allowed;
            float cx = i * slot + slot / 2f;

            float bh = (float)(day.Spent / max) * areaH;
            if (bh < 2 * den) bh = 2 * den;

            p.Color = over ? OverFill
                    : day.Spent <= 0 ? AColor.Argb(70, 255, 255, 255)
                    : isToday ? AColor.White
                    : AColor.Argb(190, 255, 255, 255);
            c.DrawRoundRect(new ARectF(cx - bw / 2, baseY - bh, cx + bw / 2, baseY), rad, rad, p);

            if (day.Spent > 0)
            {
                p.Color = AColor.Argb(235, 255, 255, 255);
                c.DrawText(Fmt.Short(day.Spent), cx, baseY - bh - 3 * den, p);
            }

            p.Color = isToday ? AColor.White : AColor.Argb(175, 255, 255, 255);
            c.DrawText(Fmt.BnDayShort(day.Day), cx, H - 2.5f * den, p);
        }
        return bmp;
    }
}
