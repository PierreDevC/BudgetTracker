using BudgetTracker.Models;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;

public class AddExpenseViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;

    string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    string _amountText = string.Empty;
    public string AmountText
    {
        get => _amountText;
        set => SetProperty(ref _amountText, value);
    }

    int _selectedCategoryIndex = -1;
    public int SelectedCategoryIndex
    {
        get => _selectedCategoryIndex;
        set => SetProperty(ref _selectedCategoryIndex, value);
    }

    DateTime _date = DateTime.Now;
    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public List<string> Categories { get; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddExpenseViewModel(Services.MockDataService data)
    {
        _data = data;
        Categories = _data.BudgetCategories.Select(c => c.Name).ToList();

        SaveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un nom.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un montant valide.", "OK");
                return;
            }

            string category = SelectedCategoryIndex >= 0 ? Categories[SelectedCategoryIndex] : "Autre";

            _data.Transactions.Insert(0, new Transaction
            {
                Name = Name,
                Amount = amount,
                Category = category,
                Date = Date,
                Type = TransactionType.Expense
            });

            await Shell.Current.Navigation.PopModalAsync();
        });

        CancelCommand = new Command(async () =>
        {
            await Shell.Current.Navigation.PopModalAsync();
        });
    }
}
