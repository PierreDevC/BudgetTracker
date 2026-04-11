namespace BudgetTracker.Views;

public partial class AddRevenuPage : ContentPage
{
    public AddRevenuPage(ViewModels.AddRevenuViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
