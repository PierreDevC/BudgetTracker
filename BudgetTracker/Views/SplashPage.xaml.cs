namespace BudgetTracker.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        NavigateAfterDelay();
    }

    async void NavigateAfterDelay()
    {
        await Task.Delay(2500);
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}