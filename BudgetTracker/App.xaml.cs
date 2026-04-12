namespace BudgetTracker;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();
        MainPage = services.GetRequiredService<Views.SplashPage>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.MinimumWidth = 440;
        window.MinimumHeight = 750;
        window.MaximumWidth = 480;
        window.MaximumHeight = 982;
        return window;
    }
}
