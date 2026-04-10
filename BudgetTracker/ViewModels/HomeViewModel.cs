using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class HomeViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;

    public string UserName => _data.CurrentUser.Name;
    public string TotalExpenses => _data.TotalExpenses.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string TotalIncome => _data.TotalIncome.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string Balance => _data.Balance.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string TransactionCount => _data.TransactionCount.ToString();

    public ICommand GoToProfileCommand { get; }
    public ICommand AddExpenseCommand { get; }

    public HomeViewModel(Services.MockDataService data)
    {
        _data = data;
        Title = "Accueil";

        GoToProfileCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("profile");
        });

        AddExpenseCommand = new Command(async () =>
        {
            string name = await Shell.Current.DisplayPromptAsync("Nouvelle dépense", "Nom:");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            string amountStr = await Shell.Current.DisplayPromptAsync("Montant", "Entrez le montant:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(amountStr, out decimal amount))
            {
                return;
            }

            string category = await Shell.Current.DisplayActionSheet("Catégorie", "Annuler", null,
                "Factures", "Besoins", "Investissements", "Envies");
            if (category == "Annuler" || string.IsNullOrEmpty(category))
            {
                return;
            }

            _data.Transactions.Insert(0, new Models.Transaction
            {
                Name = name,
                Amount = amount,
                Category = category,
                Date = DateTime.Now,
                Type = Models.TransactionType.Expense
            });

            RefreshProperties();
        });
    }

    public void RefreshProperties()
    {
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TransactionCount));
    }
}
