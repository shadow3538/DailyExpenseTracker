namespace DailyExpenseTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Theme.Apply();          // Apply saved theme
        Theme.HookSystemChange();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}
