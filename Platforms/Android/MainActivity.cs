using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace DailyExpenseTracker;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        KeyboardWatcher.Attach(this);

        if (savedInstanceState == null) Capture(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Capture(intent);
        WidgetNav.TryGo();
    }

    public override bool DispatchTouchEvent(MotionEvent? e)
    {
        try { TabSwipe.Feed(e, this); } catch { }
        return base.DispatchTouchEvent(e);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == Saf.ReqTree || requestCode == Saf.ReqCreate) Saf.Complete(resultCode, data);
    }

    static void Capture(Intent? i)
    {
        var route = i?.GetStringExtra("route");
        if (string.IsNullOrEmpty(route)) return;
        WidgetNav.Pending = route;
        i!.RemoveExtra("route");
    }
}
