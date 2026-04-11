using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour un élément de budget individuel.
/// Auteur : Pierre
/// </summary>
public class BudgetItemViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly DatabaseService _db;

    /// <summary>
    /// Obtient la catégorie de budget.
    /// </summary>
    public BudgetCategory Category { get; }

    /// <summary>
    /// Obtient le nom de la catégorie.
    /// </summary>
    public string Name => Category.Name;

    /// <summary>
    /// Obtient ou définit le montant alloué.
    /// </summary>
    public decimal Budgeted
    {
        get => Category.Amount;
        set
        {
            if (Category.Amount == value) return;
            Category.Amount = value;
            OnPropertyChanged(nameof(Budgeted));
            Refresh();
        }
    }

    /// <summary>
    /// Obtient le montant dépensé.
    /// </summary>
    public decimal Spent => _db.Transactions
        .Where(t => t.Type == TransactionType.Expense &&
                    string.Equals(t.Category, Category.Name, StringComparison.OrdinalIgnoreCase) &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    /// <summary>
    /// Obtient le budget restant.
    /// </summary>
    public decimal Remaining => Budgeted - Spent;

    /// <summary>
    /// Obtient le ratio de progression du budget.
    /// </summary>
    public double Progress => Budgeted == 0 ? 0 : Math.Min((double)(Spent / Budgeted), 1.0);

    /// <summary>
    /// Obtient la couleur de progression selon le ratio.
    /// </summary>
    public Color ProgressColor
    {
        get
        {
            if (Budgeted == 0) return Color.FromArgb("#00B894");
            var ratio = Spent / Budgeted;
            return ratio > 1.0m  ? Color.FromArgb("#FF7675")
                 : ratio > 0.75m ? Color.FromArgb("#FDCB6E")
                 : Color.FromArgb("#00B894");
        }
    }

    /// <summary>
    /// Obtient la couleur pour le montant restant.
    /// </summary>
    public Color RemainingColor => Remaining < 0
        ? Color.FromArgb("#FF7675")
        : Color.FromArgb("#134016");

    /// <summary>
    /// Obtient la chaîne de résumé pour dépensé contre budget.
    /// </summary>
    public string SpentSummary =>
        $"Budget: {Budgeted.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}  •  " +
        $"Dépensé: {Spent.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}";

    /// <summary>
    /// Initialise une nouvelle instance de BudgetItemViewModel.
    /// </summary>
    public BudgetItemViewModel(BudgetCategory category, DatabaseService db)
    {
        Category = category;
        _db = db;
    }

    /// <summary>
    /// Rafraîchit toutes les propriétés calculées.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(Spent));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressColor));
        OnPropertyChanged(nameof(RemainingColor));
        OnPropertyChanged(nameof(SpentSummary));
    }
}

/// <summary>
/// ViewModel pour gérer le budget global.
/// Auteur : Pierre
/// </summary>
public class BudgetViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly DatabaseService _db;

    /// <summary>
    /// Obtient la liste des catégories de budget.
    /// </summary>
    public ObservableCollection<BudgetItemViewModel> Categories { get; } = new();

    /// <summary>
    /// Obtient le titre du mois en cours.
    /// </summary>
    public string MonthTitle =>
        $"Budget de {DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("fr-CA"))}";

    /// <summary>
    /// Obtient ou définit le revenu mensuel total.
    /// </summary>
    public decimal TotalIncome
    {
        get => _db.CurrentUser?.MonthlyIncome ?? 0;
        set
        {
            if (_db.CurrentUser is null) return;
            _db.CurrentUser.MonthlyIncome = value;
            _ = _db.UpdateUserAsync();
            OnPropertyChanged(nameof(TotalIncome));
            RefreshTotals();
        }
    }

    /// <summary>
    /// Obtient le montant budgétisé total.
    /// </summary>
    public decimal TotalBudgeted => Categories.Sum(c => c.Budgeted);

    /// <summary>
    /// Obtient le budget restant total.
    /// </summary>
    public decimal TotalRemaining => TotalIncome - TotalBudgeted;

    /// <summary>
    /// Obtient la couleur pour le budget restant total.
    /// </summary>
    public Color TotalRemainingColor => TotalRemaining >= 0
        ? Color.FromArgb("#134016")
        : Color.FromArgb("#FF7675");

    /// <summary>
    /// Obtient une valeur indiquant s'il y a des catégories.
    /// </summary>
    public bool HasCategories => Categories.Count > 0;

    /// <summary>
    /// Obtient une valeur indiquant si la liste des catégories est vide.
    /// </summary>
    public bool IsEmpty => !HasCategories;

    /// <summary>
    /// Obtient un message d'aperçu sur l'état du budget.
    /// </summary>
    public string InsightMessage
    {
        get
        {
            var over = Categories.FirstOrDefault(c => c.Spent > c.Budgeted);
            if (over != null)
                return $"⚠️ Vous avez dépassé votre budget pour {over.Name}.";

            var near = Categories.FirstOrDefault(c => c.Budgeted > 0 && c.Spent / c.Budgeted > 0.75m);
            if (near != null)
                return $"🔶 Attention, vous approchez de la limite pour {near.Name}.";

            return string.Empty;
        }
    }

    /// <summary>
    /// Obtient une valeur indiquant s'il y a un message d'aperçu.
    /// </summary>
    public bool HasInsight => !string.IsNullOrEmpty(InsightMessage);

    /// <summary>
    /// Commande pour éditer une catégorie.
    /// </summary>
    public ICommand EditCategoryCommand { get; }

    /// <summary>
    /// Commande pour éditer le revenu total.
    /// </summary>
    public ICommand EditIncomeCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de BudgetViewModel.
    /// </summary>
    public BudgetViewModel(DatabaseService db)
    {
        _db = db;

        foreach (var cat in _db.BudgetCategories)
            Categories.Add(new BudgetItemViewModel(cat, _db));

        Categories.CollectionChanged += (_, _) => RefreshTotals();

        EditCategoryCommand = new Command<BudgetItemViewModel>(async (item) =>
        {
            string action = await Shell.Current.DisplayActionSheet(
                item.Name, "Annuler", null,
                "Modifier le montant", "Supprimer");

            if (action == "Modifier le montant")
            {
                string amountStr = await Shell.Current.DisplayPromptAsync(
                    "Modifier", $"Nouveau budget pour {item.Name} ($):",
                    initialValue: item.Budgeted.ToString("F2"),
                    keyboard: Keyboard.Numeric);

                if (decimal.TryParse(amountStr, out var amount))
                {
                    item.Budgeted = amount;
                    await _db.UpdateCategoryAsync(item.Category);
                    RefreshTotals();
                }
            }
            else if (action == "Supprimer")
            {
                await _db.DeleteCategoryAsync(item.Category);
                Categories.Remove(item);
            }
        });

        EditIncomeCommand = new Command(async () =>
        {
            string amountStr = await Shell.Current.DisplayPromptAsync(
                "Revenu mensuel", "Montant ($):",
                initialValue: TotalIncome.ToString("F2"),
                keyboard: Keyboard.Numeric);

            if (decimal.TryParse(amountStr, out var amount))
                TotalIncome = amount;
        });
    }

    /// <summary>
    /// Ajoute une nouvelle catégorie de manière asynchrone.
    /// </summary>
    public async Task AddCategoryAsync(string name, decimal amount)
    {
        var cat = new BudgetCategory { Name = name, Amount = amount };
        await _db.AddCategoryAsync(cat);
        Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
    }

    /// <summary>
    /// Rafraîchit toutes les catégories et les totaux.
    /// </summary>
    public void RefreshAll()
    {
        Categories.Clear();
        foreach (var cat in _db.BudgetCategories)
            Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
    }

    /// <summary>
    /// Rafraîchit les valeurs totales.
    /// </summary>
    void RefreshTotals()
    {
        OnPropertyChanged(nameof(TotalBudgeted));
        OnPropertyChanged(nameof(TotalRemaining));
        OnPropertyChanged(nameof(TotalRemainingColor));
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(HasCategories));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(InsightMessage));
        OnPropertyChanged(nameof(HasInsight));
    }
}