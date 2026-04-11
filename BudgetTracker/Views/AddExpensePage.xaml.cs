namespace BudgetTracker.Views;

public partial class AddExpensePage : ContentPage
{
    public AddExpensePage(ViewModels.AddExpenseViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
