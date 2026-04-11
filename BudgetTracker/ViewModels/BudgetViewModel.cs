using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetTracker.Models;
using BudgetTracker.Services;

namespace BudgetTracker.ViewModels;

public class BudgetItemViewModel : BaseViewModel
{
    readonly DatabaseService _db;

    public BudgetCategory Category { get; }
    public string Name => Category.Name;

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

    public decimal Spent => _db.Transactions
        .Where(t => t.Type == TransactionType.Expense &&
                    string.Equals(t.Category, Category.Name, StringComparison.OrdinalIgnoreCase) &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    public decimal Remaining => Budgeted - Spent;

    public double Progress => Budgeted == 0 ? 0 : Math.Min((double)(Spent / Budgeted), 1.0);

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

    public Color RemainingColor => Remaining < 0
        ? Color.FromArgb("#FF7675")
        : Color.FromArgb("#134016");

    public string SpentSummary =>
        $"Budget: {Budgeted.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}  •  " +
        $"Dépensé: {Spent.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}";

    public BudgetItemViewModel(BudgetCategory category, DatabaseService db)
    {
        Category = category;
        _db = db;
    }

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

public class BudgetViewModel : BaseViewModel
{
    readonly DatabaseService _db;

    public ObservableCollection<BudgetItemViewModel> Categories { get; } = new();

    public string MonthTitle =>
        $"Budget de {DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("fr-CA"))}";

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

    public decimal TotalBudgeted => Categories.Sum(c => c.Budgeted);
    public decimal TotalRemaining => TotalIncome - TotalBudgeted;

    public Color TotalRemainingColor => TotalRemaining >= 0
        ? Color.FromArgb("#134016")
        : Color.FromArgb("#FF7675");

    public bool HasCategories => Categories.Count > 0;
    public bool IsEmpty => !HasCategories;

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

    public bool HasInsight => !string.IsNullOrEmpty(InsightMessage);

    public ICommand EditCategoryCommand { get; }
    public ICommand EditIncomeCommand { get; }

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

    public async Task AddCategoryAsync(string name, decimal amount)
    {
        var cat = new BudgetCategory { Name = name, Amount = amount };
        await _db.AddCategoryAsync(cat);
        Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
    }

    public void RefreshAll()
    {
        Categories.Clear();
        foreach (var cat in _db.BudgetCategories)
            Categories.Add(new BudgetItemViewModel(cat, _db));
        RefreshTotals();
    }

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
