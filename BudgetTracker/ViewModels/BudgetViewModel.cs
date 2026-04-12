using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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
    /// <remarks>
    /// Déclenche un rafraîchissement des propriétés calculées quand la valeur change.
    /// </remarks>
    public decimal Budgeted
    {
        get => Category.Amount;
        set
        {
            // Évite les mises à jour inutiles si la valeur n'a pas changé
            if (Category.Amount == value) return;
            Category.Amount = value;
            OnPropertyChanged(nameof(Budgeted));
            // Rafraîchit toutes les propriétés dérivées (Remaining, Progress, etc.)
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
    /// <remarks>
    /// Retourne du rouge (dépassé), jaune (près de la limite) ou vert (dans les limites).
    /// </remarks>
    public Color ProgressColor
    {
        get
        {
            // Budget non défini = vert (pas de risque)
            if (Budgeted == 0) return Color.FromArgb("#00B894");
            var ratio = Spent / Budgeted;
            // Rouge si dépassé, jaune si >= 75%, vert sinon
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
    /// <param name="category">La catégorie de budget associée.</param>
    /// <param name="db">Le service de base de données pour consulter les transactions.</param>
    public BudgetItemViewModel(BudgetCategory category, DatabaseService db)
    {
        Category = category;
        _db = db;
    }

    /// <summary>
    /// Rafraîchit toutes les propriétés calculées.
    /// </summary>
    /// <remarks>
    /// Appelé après une modification du montant budgétisé pour mettre à jour
    /// les propriétés dérivées (montant dépensé, solde, couleurs).
    /// </remarks>
    public void Refresh()
    {
        // Notifie des montants et du ratios de progression
        OnPropertyChanged(nameof(Spent));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(Progress));

        // Notifie des changements de couleur selon l'état du budget
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
    /// <remarks>
    /// Persiste automatiquement les changements à la base de données
    /// et rafraîchit les propriétés calculées.
    /// </remarks>
    public decimal TotalIncome
    {
        get => _db.CurrentUser?.MonthlyIncome ?? 0;
        set
        {
            if (_db.CurrentUser is null) return;
            // Met à jour le revenu de l'utilisateur
            _db.CurrentUser.MonthlyIncome = value;
            // Enregistre le changement en base de données (fire-and-forget)
            _ = _db.UpdateUserAsync();
            OnPropertyChanged(nameof(TotalIncome));
            // Rafraîchit le budget restant et les indicateurs visuels
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
    /// <remarks>
    /// Retourne un message d'alerte si une catégorie a été dépassée,
    /// un avertissement si une approche les 75% du budget, ou une chaîne vide sinon.
    /// </remarks>
    public string InsightMessage
    {
        get
        {
            // Vérifie d'abord si une catégorie a été dépassée (priorité haute)
            var over = Categories.FirstOrDefault(c => c.Spent > c.Budgeted);
            if (over != null)
                return $"⚠️ Vous avez dépassé votre budget pour {over.Name}.";

            // Vérifie ensuite si une catégorie approche de la limite (75% dépensé)
            var near = Categories.FirstOrDefault(c => c.Budgeted > 0 && c.Spent / c.Budgeted > 0.75m);
            if (near != null)
                return $"🔶 Attention, vous approchez de la limite pour {near.Name}.";

            // Pas de message si tout est dans les limites
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
    /// <param name="db">Le service de base de données.</param>
    public BudgetViewModel(DatabaseService db)
    {
        _db = db;

        foreach (var cat in _db.BudgetCategories)
            Categories.Add(new BudgetItemViewModel(cat, _db));

        Categories.CollectionChanged += (_, _) => RefreshTotals();

        EditCategoryCommand = new Command<BudgetItemViewModel>(async (item) =>
        {
            // Affiche un menu d'actions pour la catégorie sélectionnée
            string action = await Shell.Current.DisplayActionSheet(
                item.Name, "Annuler", null,
                "Modifier le montant", "Supprimer");

            // Traite l'action de modification du montant
            if (action == "Modifier le montant")
            {
                string amountStr = await Shell.Current.DisplayPromptAsync(
                    "Modifier", $"Nouveau budget pour {item.Name} ($):",
                    initialValue: item.Budgeted.ToString("F2"),
                    keyboard: Keyboard.Numeric);

                // Valide et enregistre le nouveau montant
                if (decimal.TryParse(amountStr, out var amount))
                {
                    item.Budgeted = amount;
                    await _db.UpdateCategoryAsync(item.Category);
                    RefreshTotals();
#if !WINDOWS
                    await Toast.Make($"Budget « {item.Name} » mis à jour.", ToastDuration.Short).Show();
#endif
                }
            }
            // Traite l'action de suppression
            else if (action == "Supprimer")
            {
                await _db.DeleteCategoryAsync(item.Category);
                Categories.Remove(item);
#if !WINDOWS
                await Toast.Make($"Catégorie « {item.Name} » supprimée.", ToastDuration.Short).Show();
#endif
            }
        });

        EditIncomeCommand = new Command(async () =>
        {
            // Invite l'utilisateur à entrer le montant du revenu mensuel
            string amountStr = await Shell.Current.DisplayPromptAsync(
                "Revenu mensuel", "Montant ($):",
                initialValue: TotalIncome.ToString("F2"),
                keyboard: Keyboard.Numeric);

            // Valide et enregistre le nouveau revenu
            if (decimal.TryParse(amountStr, out var amount))
            {
                TotalIncome = amount;
#if !WINDOWS
                await Toast.Make("Revenu mensuel mis à jour.", ToastDuration.Short).Show();
#endif
            }
        });
    }

    /// <summary>
    /// Ajoute une nouvelle catégorie de manière asynchrone.
    /// </summary>
    /// <param name="name">Le nom de la catégorie.</param>
    /// <param name="amount">Le montant du budget alloué.</param>
    public async Task AddCategoryAsync(string name, decimal amount)
    {
        var cat = new BudgetCategory { Name = name, Amount = amount };
        await _db.AddCategoryAsync(cat);
        Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
#if !WINDOWS
        await Toast.Make($"Catégorie « {name} » ajoutée.", ToastDuration.Short).Show();
#endif
    }

    /// <summary>
    /// Rafraîchit toutes les catégories et les totaux.
    /// </summary>
    /// <remarks>
    /// Recrée la liste complète de BudgetItemViewModel à partir de la base de données,
    /// utile après une synchronisation ou une grande modification.
    /// </remarks>
    public void RefreshAll()
    {
        // Efface et recrée la collection de catégories depuis la base de données
        Categories.Clear();
        foreach (var cat in _db.BudgetCategories)
            Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
    }

    /// <summary>
    /// Rafraîchit les valeurs totales.
    /// </summary>
    /// <remarks>
    /// Notifie la vue de tous les changements de propriétés calculées.
    /// Appelé après toute modification de catégorie ou revenu.
    /// </remarks>
    void RefreshTotals()
    {
        // Notifie des changements de montants et solde
        OnPropertyChanged(nameof(TotalBudgeted));
        OnPropertyChanged(nameof(TotalRemaining));
        OnPropertyChanged(nameof(TotalRemainingColor));
        OnPropertyChanged(nameof(TotalIncome));

        // Notifie des changements de visibilité et messages
        OnPropertyChanged(nameof(HasCategories));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(InsightMessage));
        OnPropertyChanged(nameof(HasInsight));
    }
}