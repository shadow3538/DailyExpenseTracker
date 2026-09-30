using Android.App;
using Android.Runtime;
using DailyExpenseTracker.Data;

namespace DailyExpenseTracker;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();

        // Widget refresh
        Store.Changed += () => ExpenseWidget.Refresh(this);

        // Alarm restore
        _ = Reminders.RescheduleAsync();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
