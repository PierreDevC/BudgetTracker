using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

public class HomeViewModel : BaseViewModel
{
    readonly Services.DatabaseService _db;
    readonly IServiceProvider _services;

    static readonly System.Globalization.CultureInfo FrCA = new("fr-CA");

    public string UserName => _db.CurrentUser?.Name ?? string.Empty;
    public string TotalExpenses => _db.TotalExpenses.ToString("C", FrCA);
    public string TotalIncome => _db.TotalIncome.ToString("C", FrCA);
    public string Balance
    {
        get
        {
            if (_db.CurrentUser?.MonthlyIncome == 0 && _db.Transactions.Count == 0)
                return "À définir";
            return _db.Balance.ToString("C", FrCA);
        }
    }
    public string TransactionCount => _db.TransactionCount.ToString();
    public IEnumerable<Models.Transaction> RecentTransactions => _db.Transactions.Take(10);
    public bool HasTransactions => _db.Transactions.Count > 0;
    public bool NoTransactions => _db.Transactions.Count == 0;

    public ICommand GoToProfileCommand { get; }
    public ICommand AddExpenseCommand { get; }
    public ICommand AddRevenuCommand { get; }
    public ICommand GoToTransactionsCommand { get; }

    public HomeViewModel(Services.DatabaseService db, IServiceProvider services)
    {
        _db = db;
        _services = services;
        Title = "Accueil";

        GoToProfileCommand = new Command(async () =>
        {
            var page = _services.GetRequiredService<Views.ProfilePage>();
            await Shell.Current.Navigation.PushAsync(page);
        });

        GoToTransactionsCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//Transactions");
        });

        AddExpenseCommand = new Command(async () =>
        {
            var page = _services.GetRequiredService<Views.AddExpensePage>();
            page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()
                .SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
            await Shell.Current.Navigation.PushModalAsync(page);
        });

        AddRevenuCommand = new Command(async () =>
        {
            var page = _services.GetRequiredService<Views.AddRevenuPage>();
            page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()
                .SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
            await Shell.Current.Navigation.PushModalAsync(page);
        });
    }

    public void RefreshProperties()
    {
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TransactionCount));
        OnPropertyChanged(nameof(RecentTransactions));
        OnPropertyChanged(nameof(HasTransactions));
        OnPropertyChanged(nameof(NoTransactions));
    }
}
