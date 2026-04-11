using BudgetTracker.Models;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;

public class AddRevenuViewModel : BaseViewModel
{
    readonly Services.DatabaseService _db;

    string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _amountText = string.Empty;
    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }

    DateTime _date = DateTime.Now;
    public DateTime Date { get => _date; set => SetProperty(ref _date, value); }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddRevenuViewModel(Services.DatabaseService db)
    {
        _db = db;

        SaveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer une source de revenu.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un montant valide.", "OK");
                return;
            }

            await _db.AddTransactionAsync(new Transaction
            {
                Name = Name,
                Amount = amount,
                Category = "Revenu",
                Date = Date,
                Type = TransactionType.Income
            });

            await Shell.Current.Navigation.PopModalAsync();
        });

        CancelCommand = new Command(async () =>
        {
            await Shell.Current.Navigation.PopModalAsync();
        });
    }
}
