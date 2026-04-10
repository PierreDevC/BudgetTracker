namespace BudgetTracker.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ViewModels.ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}