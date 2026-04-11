namespace BudgetTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        this.InitializeComponent();

        Routing.RegisterRoute("profile", typeof(Views.ProfilePage));
    }
}