namespace BudgetTracker.Views;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage(ViewModels.OnboardingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
