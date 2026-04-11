using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class TransactionsViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;

    public ObservableCollection<Models.Transaction> Transactions { get; } = new();

    public ICommand AddTransactionCommand { get; }

    public TransactionsViewModel(Services.MockDataService data)
    {
        _data = data;
        Title = "Transactions";

        foreach (var t in _data.Transactions.OrderByDescending(t => t.Date))
            Transactions.Add(t);

        AddTransactionCommand = new Command(async () =>
        {
            string name = await Shell.Current.DisplayPromptAsync("Nouvelle transaction", "Nom:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            string typeStr = await Shell.Current.DisplayActionSheet("Type", "Annuler", null, "Dépense", "Revenu");
            if (typeStr == "Annuler" || string.IsNullOrEmpty(typeStr))
            {
                return;
            }

            string amountStr = await Shell.Current.DisplayPromptAsync("Montant", "Montant:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(amountStr, out decimal amount))
            {
                return;
            }

            string category = await Shell.Current.DisplayActionSheet("Catégorie", "Annuler", null,
                "Factures", "Besoins", "Investissements", "Envies", "Revenu");
            if (category == "Annuler" || string.IsNullOrEmpty(category))
            {
                return;
            }

            var tx = new Models.Transaction
            {
                Name = name,
                Amount = amount,
                Category = category,
                Date = DateTime.Now,
                Type = typeStr == "Revenu" ? Models.TransactionType.Income : Models.TransactionType.Expense
            };
            _data.Transactions.Insert(0, tx);
            Transactions.Insert(0, tx);
        });
    }

    public void Refresh()
    {
        Transactions.Clear();
        foreach (var t in _data.Transactions.OrderByDescending(t => t.Date))
            Transactions.Add(t);
    }
}
