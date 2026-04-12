using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour le tableau de bord principal.
/// Auteur : Pierre
/// </summary>
public class HomeViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

    /// <summary>
    /// Instance du fournisseur de services.
    /// </summary>
    readonly IServiceProvider _services;

    /// <summary>
    /// Informations culturelles pour le français canadien.
    /// </summary>
    static readonly System.Globalization.CultureInfo FrCA = new("fr-CA");

    /// <summary>
    /// Obtient le nom de l'utilisateur actuel.
    /// </summary>
    public string UserName => _db.CurrentUser?.Name ?? string.Empty;

    /// <summary>
    /// Obtient le total des dépenses.
    /// </summary>
    public string TotalExpenses => _db.TotalExpenses.ToString("C", FrCA);

    /// <summary>
    /// Obtient le revenu total.
    /// </summary>
    public string TotalIncome => _db.TotalIncome.ToString("C", FrCA);

    /// <summary>
    /// Obtient le solde actuel.
    /// </summary>
    /// <remarks>
    /// Affiche "À définir" si aucun revenu ni transaction n'a été configuré,
    /// sinon affiche le solde formaté en devise canadienne-française.
    /// </remarks>
    public string Balance
    {
        get
        {
            // Affiche "À définir" si l'utilisateur n'a pas configuré de revenu et aucune transaction
            if (_db.CurrentUser?.MonthlyIncome == 0 && _db.Transactions.Count == 0)
                return "À définir";
            return _db.Balance.ToString("C", FrCA);
        }
    }

    /// <summary>
    /// Obtient le nombre de transactions.
    /// </summary>
    public string TransactionCount => _db.TransactionCount.ToString();

    /// <summary>
    /// Obtient la liste des transactions récentes.
    /// </summary>
    public IEnumerable<Models.Transaction> RecentTransactions => _db.Transactions.Take(10);

    /// <summary>
    /// Obtient une valeur indiquant s'il y a des transactions.
    /// </summary>
    public bool HasTransactions => _db.Transactions.Count > 0;

    /// <summary>
    /// Obtient une valeur indiquant s'il n'y a aucune transaction.
    /// </summary>
    public bool NoTransactions => _db.Transactions.Count == 0;

    /// <summary>
    /// Commande pour naviguer vers le profil.
    /// </summary>
    public ICommand GoToProfileCommand { get; }

    /// <summary>
    /// Commande pour ajouter une nouvelle dépense.
    /// </summary>
    public ICommand AddExpenseCommand { get; }

    /// <summary>
    /// Commande pour ajouter un nouveau revenu.
    /// </summary>
    public ICommand AddRevenuCommand { get; }

    /// <summary>
    /// Commande pour naviguer vers les transactions.
    /// </summary>
    public ICommand GoToTransactionsCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de HomeViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    /// <param name="services">Le fournisseur de services pour la navigation.</param>
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

    /// <summary>
    /// Rafraîchit toutes les propriétés liées à l'accueil.
    /// </summary>
    /// <remarks>
    /// Utilisé lors de l'apparition de la page pour mettre à jour les données calculées
    /// qui dépendent du service de base de données.
    /// </remarks>
    public void RefreshProperties()
    {
        // Notifie la vue que les propriétés utilisateur et calculs financiers ont changé
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TransactionCount));

        // Notifie la vue des collections et booléens de visibilité
        OnPropertyChanged(nameof(RecentTransactions));
        OnPropertyChanged(nameof(HasTransactions));
        OnPropertyChanged(nameof(NoTransactions));
    }
}