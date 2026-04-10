namespace BudgetTracker.Views;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage(ViewModels.StatisticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}