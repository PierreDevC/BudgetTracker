namespace BudgetTracker;

public partial class AppShell : Shell
{
    public AppShell(
        Views.HomePage homePage,
        Views.BudgetPage budgetPage,
        Views.TransactionsPage transactionsPage,
        Views.StatisticsPage statisticsPage)
    {
        InitializeComponent();
        HomeTab.Content = homePage;
        BudgetTab.Content = budgetPage;
        TransactionsTab.Content = transactionsPage;
        StatistiquesTab.Content = statisticsPage;
    }
}
