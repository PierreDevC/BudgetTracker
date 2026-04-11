using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

public class HomeViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;
    readonly IServiceProvider _services;

    public string UserName => _data.CurrentUser.Name;
    public string TotalExpenses => _data.TotalExpenses.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string TotalIncome => _data.TotalIncome.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string Balance => _data.Balance.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string TransactionCount => _data.TransactionCount.ToString();
    public IEnumerable<Models.Transaction> RecentTransactions => _data.Transactions.Take(10);
    public bool HasTransactions => _data.Transactions.Count > 0;
    public bool NoTransactions => _data.Transactions.Count == 0;

    public ICommand GoToProfileCommand { get; }
    public ICommand AddExpenseCommand { get; }
    public ICommand GoToTransactionsCommand { get; }

    public HomeViewModel(Services.MockDataService data, IServiceProvider services)
    {
        _data = data;
        _services = services;
        Title = "Accueil";

        GoToProfileCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("profile");
        });

        GoToTransactionsCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//Transactions");
        });

        AddExpenseCommand = new Command(async () =>
        {
            var page = _services.GetRequiredService<Views.AddExpensePage>();
            page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()
                .SetModalPresentationStyle(
                    Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.PageSheet);
            await Shell.Current.Navigation.PushModalAsync(page);
        });
    }

    public void RefreshProperties()
    {
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TransactionCount));
        OnPropertyChanged(nameof(RecentTransactions));
        OnPropertyChanged(nameof(HasTransactions));
        OnPropertyChanged(nameof(NoTransactions));
    }
}
