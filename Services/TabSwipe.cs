using Android.Views;

namespace DailyExpenseTracker;

// Horizontal swipe between
public static class TabSwipe
{
    static readonly string[] Order = { "home", "add", "report", "calc", "settings" };
    static readonly List<WeakReference<VisualElement>> Guards = new();

    static float _x0, _y0;
    static long _t0;
    static bool _track;

    // Views that scroll
    public static void Guard(VisualElement v)
    {
        Guards.RemoveAll(w => !w.TryGetTarget(out _));
        Guards.Add(new WeakReference<VisualElement>(v));
    }

    public static void Feed(MotionEvent? e, global::Android.Content.Context ctx)
    {
        if (e == null) return;
        switch (e.ActionMasked)
        {
            case MotionEventActions.Down:
                _x0 = e.RawX; _y0 = e.RawY; _t0 = e.EventTime;
                _track = Allowed() && !InGuard(e.RawX, e.RawY);
                break;

            case MotionEventActions.PointerDown:
            case MotionEventActions.Cancel:
                _track = false;
                break;

            case MotionEventActions.Up:
                if (!_track) return;
                _track = false;
                float dens = ctx.Resources?.DisplayMetrics?.Density ?? 2f;
                float dx = e.RawX - _x0, dy = e.RawY - _y0;
                if (Math.Abs(dx) < 90 * dens) return;          // too short
                if (Math.Abs(dx) < Math.Abs(dy) * 2.2f) return; // not horizontal
                if (e.EventTime - _t0 > 700) return;            // too slow
                Fire(dx < 0 ? 1 : -1);
                break;
        }
    }

    static bool Allowed()
    {
        try
        {
            var a = RootChrome.Active;
            if (a == null || a.IsBusy) return false;
            var shell = Shell.Current;
            if (shell == null || shell.Navigation.ModalStack.Count > 0) return false;
            return true;
        }
        catch { return false; }
    }

    static bool InGuard(float x, float y)
    {
        try
        {
            foreach (var w in Guards)
            {
                if (!w.TryGetTarget(out var v)) continue;
                if (v.Handler?.PlatformView is not global::Android.Views.View nv || !nv.IsShown) continue;
                var loc = new int[2];
                nv.GetLocationOnScreen(loc);
                if (x >= loc[0] && x <= loc[0] + nv.Width && y >= loc[1] && y <= loc[1] + nv.Height) return true;
            }
        }
        catch { }
        return false;
    }

    static void Fire(int step)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var a = RootChrome.Active;
                if (a == null) return;
                if (a.IsOpen)
                {
                    if (step > 0) _ = a.CloseAsync();   // swipe left closes
                    return;
                }
                int i = Array.IndexOf(Order, Ui.CurrentRoute ?? "home");
                if (i < 0) return;
                int n = i + step;
                if (n < 0) { _ = a.OpenAsync(); return; }   // first tab: open drawer
                if (n >= Order.Length) return;
                _ = Ui.GoTo(Order[n]);
            }
            catch (Exception ex) { AppLog.Error("TabSwipe", ex); }
        });
    }
}

// Detects soft keyboard
public static class KeyboardWatcher
{
    public static bool Visible { get; private set; }
    public static double HeightDp { get; private set; }

    // Pages subscribe in
    public static event Action? Changed;

    // Single handler owned
    public static Action? ShellHandler;

    static bool _hooked;

    public static void Attach(global::Android.App.Activity act)
    {
        if (_hooked) return;
        var decor = act.Window?.DecorView;
        var obs = decor?.ViewTreeObserver;
        if (decor == null || obs == null) return;
        _hooked = true;
        obs.GlobalLayout += (_, _) => Check(decor);
    }

    static void Check(global::Android.Views.View decor)
    {
        try
        {
            var r = new global::Android.Graphics.Rect();
            decor.GetWindowVisibleDisplayFrame(r);
            int total = decor.RootView?.Height ?? decor.Height;
            if (total <= 0) return;
            int hidden = total - r.Bottom;
            bool vis = hidden > total * 0.15;
            float dens = decor.Resources?.DisplayMetrics?.Density ?? 2f;
            double dp = vis ? hidden / dens : 0;
            if (vis == Visible && Math.Abs(dp - HeightDp) < 2) return;
            Visible = vis;
            HeightDp = dp;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try { ShellHandler?.Invoke(); } catch { }
                try { Changed?.Invoke(); } catch { }
            });
        }
        catch (Exception ex) { AppLog.Error("Keyboard", ex); }
    }
}
