using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

public class TransactionsViewModel : BaseViewModel
{
    readonly Services.DatabaseService _db;
    readonly IServiceProvider _services;

    public ObservableCollection<Models.Transaction> Transactions { get; } = new();

    public ICommand AddTransactionCommand { get; }

    public TransactionsViewModel(Services.DatabaseService db, IServiceProvider services)
    {
        _db = db;
        _services = services;
        Title = "Transactions";

        foreach (var t in _db.Transactions.OrderByDescending(t => t.Date))
            Transactions.Add(t);

        AddTransactionCommand = new Command(async () =>
        {
            var page = _services.GetRequiredService<Views.AddExpensePage>();
            page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>()
                .SetModalPresentationStyle(
                    Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.PageSheet);
            await Shell.Current.Navigation.PushModalAsync(page);
        });
    }

    public void Refresh()
    {
        Transactions.Clear();
        foreach (var t in _db.Transactions.OrderByDescending(t => t.Date))
            Transactions.Add(t);
    }
}
