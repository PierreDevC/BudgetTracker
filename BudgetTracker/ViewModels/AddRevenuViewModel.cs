using BudgetTracker.Models;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour l'ajout d'un nouveau revenu.
/// Auteur : Pierre
/// </summary>
public class AddRevenuViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

    string _name = string.Empty;
    /// <summary>
    /// Obtient ou définit le nom du revenu.
    /// </summary>
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _amountText = string.Empty;
    /// <summary>
    /// Obtient ou définit le texte du montant du revenu.
    /// </summary>
    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }

    DateTime _date = DateTime.Now;
    /// <summary>
    /// Obtient ou définit la date du revenu.
    /// </summary>
    public DateTime Date { get => _date; set => SetProperty(ref _date, value); }

    /// <summary>
    /// Commande pour sauvegarder le revenu.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Commande pour annuler l'opération.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de AddRevenuViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    public AddRevenuViewModel(Services.DatabaseService db)
    {
        _db = db;

        SaveCommand = new Command(async () =>
        {
            // Valide que la source de revenu n'est pas vide
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer une source de revenu.", "OK");
                return;
            }

            // Valide que le montant est un nombre positif valide
            if (!decimal.TryParse(AmountText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un montant valide.", "OK");
                return;
            }

            // Crée et enregistre la transaction de revenu en base de données
            await _db.AddTransactionAsync(new Transaction
            {
                Name = Name,
                Amount = amount,
                Category = "Revenu",  // Catégorie fixe pour les revenus
                Date = Date,
                Type = TransactionType.Income
            });

            // Ferme la page modale après l'enregistrement
            await Shell.Current.Navigation.PopModalAsync();
        });

        CancelCommand = new Command(async () =>
        {
            await Shell.Current.Navigation.PopModalAsync();
        });
    }
}