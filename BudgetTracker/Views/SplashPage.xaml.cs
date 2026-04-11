namespace BudgetTracker.Views;

public partial class SplashPage : ContentPage
{
    readonly Services.AuthService _auth;
    readonly IServiceProvider _services;

    public SplashPage(Services.AuthService auth, IServiceProvider services)
    {
        InitializeComponent();
        _auth = auth;
        _services = services;
        NavigateAfterDelay();
    }

    async void NavigateAfterDelay()
    {
        await Task.Delay(2000);
        bool restored = await _auth.TryRestoreSessionAsync();
        Application.Current!.MainPage = restored
            ? _services.GetRequiredService<AppShell>()
            : new NavigationPage(_services.GetRequiredService<LoginPage>());
    }
}
