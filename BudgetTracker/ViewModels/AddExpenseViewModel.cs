using BudgetTracker.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour l'ajout d'une nouvelle dépense.
/// Auteur : Pierre
/// </summary>
public class AddExpenseViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

    string _name = string.Empty;
    /// <summary>
    /// Obtient ou définit le nom de la dépense.
    /// </summary>
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _amountText = string.Empty;
    /// <summary>
    /// Obtient ou définit le texte du montant de la dépense.
    /// </summary>
    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }

    int _selectedCategoryIndex = -1;
    /// <summary>
    /// Obtient ou définit l'index de la catégorie sélectionnée.
    /// </summary>
    public int SelectedCategoryIndex { get => _selectedCategoryIndex; set => SetProperty(ref _selectedCategoryIndex, value); }

    DateTime _date = DateTime.Now;
    /// <summary>
    /// Obtient ou définit la date de la dépense.
    /// </summary>
    public DateTime Date { get => _date; set => SetProperty(ref _date, value); }

    /// <summary>
    /// Obtient la liste des catégories disponibles.
    /// </summary>
    public List<string> Categories { get; }

    /// <summary>
    /// Commande pour sauvegarder la dépense.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Commande pour annuler l'opération.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de AddExpenseViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    public AddExpenseViewModel(Services.DatabaseService db)
    {
        _db = db;
        Categories = _db.BudgetCategories.Select(c => c.Name).ToList();

        SaveCommand = new Command(async () =>
        {
            // Valide que le nom de la dépense n'est pas vide
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un nom.", "OK");
                return;
            }

            // Valide que le montant est un nombre positif valide
            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un montant valide.", "OK");
                return;
            }

            // Utilise la catégorie sélectionnée ou "Autre" par défaut
            string category = SelectedCategoryIndex >= 0 ? Categories[SelectedCategoryIndex] : "Autre";

            // Crée et enregistre la transaction en base de données
            await _db.AddTransactionAsync(new Transaction
            {
                Name = Name,
                Amount = amount,
                Category = category,
                Date = Date,
                Type = TransactionType.Expense
            });

            // Ferme la page modale, puis affiche un toast de confirmation
            await Shell.Current.Navigation.PopModalAsync();
            await Toast.Make("Dépense ajoutée.", ToastDuration.Short).Show();
        });

        CancelCommand = new Command(async () =>
        {
            await Shell.Current.Navigation.PopModalAsync();
        });
    }
}