namespace DailyExpenseTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // Apply theme
        Theme.Apply();
        Theme.HookSystemChange();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}
