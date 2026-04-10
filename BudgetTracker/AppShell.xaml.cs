namespace BudgetTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("profile", typeof(Views.ProfilePage));
    }
}