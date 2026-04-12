using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour la gestion des transactions.
/// Auteur : Pierre
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
    /// Obtient la collection des transactions.
    /// </summary>
    public ObservableCollection<Models.Transaction> Transactions { get; } = new();

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

    /// <summary>
    /// Rafraîchit la liste des transactions.
    /// </summary>
    /// <remarks>
    /// Recrée la collection en triant par date décroissante (les plus récentes en premier).
    /// Appelé lors de l'apparition de la page pour afficher les derniers changements.
    /// </remarks>
    public void Refresh()
    {
        // Vide et recrée la liste de transactions depuis la base de données
        Transactions.Clear();
        // Trie par date décroissante (les plus récentes d'abord)
        foreach (var t in _db.Transactions.OrderByDescending(t => t.Date))
            Transactions.Add(t);
    }
}