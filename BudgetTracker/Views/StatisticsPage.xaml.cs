namespace BudgetTracker.Views;

public partial class StatisticsPage : ContentPage
{
    readonly ViewModels.StatisticsViewModel _vm;

    public StatisticsPage(ViewModels.StatisticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }
}