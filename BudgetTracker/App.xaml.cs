namespace BudgetTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new Views.SplashPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        window.MinimumWidth  = 390;
        window.MinimumHeight = 700;
        window.MaximumWidth  = 430;
        window.MaximumHeight = 932;

        return window;
    }
}