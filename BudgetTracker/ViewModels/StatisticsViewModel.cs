using System.Collections.ObjectModel;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour afficher les statistiques.
/// Auteur : Aboubacar
/// </summary>
public class StatisticsViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

    /// <summary>
    /// Informations culturelles pour le français canadien.
    /// </summary>
    static readonly System.Globalization.CultureInfo FrCA = new("fr-CA");

    /// <summary>
    /// Obtient le total des dépenses du mois.
    /// </summary>
    public string TotalExpenses => _db.TotalExpenses.ToString("C", FrCA);

    /// <summary>
    /// Obtient le total des revenus du mois.
    /// </summary>
    public string TotalIncome => _db.TotalIncome.ToString("C", FrCA);

    /// <summary>
    /// Obtient le solde du mois.
    /// </summary>
    public string Balance => _db.Balance.ToString("C", FrCA);

    /// <summary>
    /// Obtient le nombre de transactions du mois.
    /// </summary>
    public int TransactionCount => _db.TransactionCount;

    /// <summary>
    /// Obtient les statistiques par catégorie.
    /// </summary>
    public ObservableCollection<CategoryStatistic> CategoryStats { get; } = new();

    /// <summary>
    /// Indique s'il y a des données à afficher.
    /// </summary>
    public bool HasData => _db.TransactionCount > 0;

    /// <summary>
    /// Indique s'il n'y a pas de données.
    /// </summary>
    public bool NoData => _db.TransactionCount == 0;

    /// <summary>
    /// Initialise une nouvelle instance de StatisticsViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    public StatisticsViewModel(Services.DatabaseService db)
    {
        _db = db;
        Title = "Statistiques";
        LoadStatistics();
    }

    /// <summary>
    /// Charge les statistiques par catégorie.
    /// </summary>
    private void LoadStatistics()
    {
        CategoryStats.Clear();

        // Groupe les dépenses par catégorie
        var expensesByCategory = _db.Transactions
            .Where(t => t.Type == Models.TransactionType.Expense &&
                        t.Date.Month == DateTime.Now.Month &&
                        t.Date.Year == DateTime.Now.Year)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryStatistic
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        foreach (var stat in expensesByCategory)
        {
            CategoryStats.Add(stat);
        }
    }

    /// <summary>
    /// Rafraîchit les statistiques.
    /// </summary>
    public void Refresh()
    {
        LoadStatistics();
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TransactionCount));
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(NoData));
    }
}

/// <summary>
/// Représente une statistique par catégorie.
/// </summary>
public class CategoryStatistic
{
    /// <summary>
    /// Obtient ou définit le nom de la catégorie.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le montant total des dépenses.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre de transactions.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Obtient le montant formaté.
    /// </summary>
    public string FormattedAmount => Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"));

    /// <summary>
    /// Obtient l'emoji associé à la catégorie.
    /// </summary>
    public string CategoryEmoji => Category switch
    {
        "Alimentation" or "Épiceries" => "🍔",
        "Transport" => "🚗",
        "Loyer" or "Logement" => "🏠",
        "Divertissement" or "Loisirs" => "🎮",
        "Santé" => "💊",
        "Vêtements" => "👕",
        "Éducation" => "📚",
        _ => "💰"
    };
}