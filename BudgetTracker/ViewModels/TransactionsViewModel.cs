using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour la gestion des transactions.
/// Auteur : Pierre, Aboubacar (recherche)
/// </summary>
public class TransactionsViewModel : BaseViewModel
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
    /// Liste complète des transactions (non filtrée).
    /// </summary>
    private List<Models.Transaction> _allTransactions = new();

    /// <summary>
    /// Obtient la collection des transactions affichées.
    /// </summary>
    public ObservableCollection<Models.Transaction> Transactions { get; } = new();

    /// <summary>
    /// Texte de recherche saisi par l'utilisateur.
    /// </summary>
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTransactions();
            }
        }
    }

    /// <summary>
    /// Commande pour ajouter une transaction.
    /// </summary>
    public ICommand AddTransactionCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de TransactionsViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    /// <param name="services">Le fournisseur de services pour la navigation.</param>
    public TransactionsViewModel(Services.DatabaseService db, IServiceProvider services)
    {
        _db = db;
        _services = services;
        Title = "Transactions";

        // Charge toutes les transactions
        _allTransactions = _db.Transactions.OrderByDescending(t => t.Date).ToList();

        foreach (var t in _allTransactions)
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

    /// <summary>
    /// Filtre les transactions selon le texte de recherche.
    /// </summary>
    private void FilterTransactions()
    {
        Transactions.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Affiche toutes les transactions si la recherche est vide
            foreach (var t in _allTransactions)
                Transactions.Add(t);
        }
        else
        {
            // Filtre par nom, catégorie ou montant
            var filtered = _allTransactions.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Amount.ToString().Contains(SearchText)
            );

            foreach (var t in filtered)
                Transactions.Add(t);
        }
    }

    /// <summary>
    /// Rafraîchit la liste des transactions.
    /// </summary>
    /// <remarks>
    /// Recrée la collection en triant par date décroissante (les plus récentes en premier).
    /// Appelé lors de l'apparition de la page pour afficher les derniers changements.
    /// </remarks>
    public void Refresh()
    {
        // Recharge toutes les transactions depuis la base de données
        _allTransactions = _db.Transactions.OrderByDescending(t => t.Date).ToList();

        // Réapplique le filtre de recherche
        FilterTransactions();
    }
}