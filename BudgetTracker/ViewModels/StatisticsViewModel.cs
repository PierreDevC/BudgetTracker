using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.ViewModels;
public class StatisticsViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;

    public string TotalExpenses => _data.TotalExpenses.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string TotalIncome => _data.TotalIncome.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string Balance => _data.Balance.ToString("C", new System.Globalization.CultureInfo("fr-CA"));

    // Category breakdown (percentage bars)
    public List<CategoryStat> CategoryBreakdown { get; } = new();

    public StatisticsViewModel(Services.MockDataService data)
    {
        _data = data;
        Title = "Statistiques";

        var expenses = _data.Transactions
            .Where(t => t.Type == Models.TransactionType.Expense && t.Date.Month == DateTime.Now.Month)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryStat
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount),
                Percentage = _data.TotalExpenses > 0
                    ? (double)(g.Sum(t => t.Amount) / _data.TotalExpenses * 100)
                    : 0,
                Color = g.Key switch
                {
                    "Factures" => Color.FromArgb("#6C63FF"),
                    "Besoins" => Color.FromArgb("#00C9A7"),
                    "Investissements" => Color.FromArgb("#FDCB6E"),
                    _ => Color.FromArgb("#FF6584"),
                }
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        CategoryBreakdown = expenses;
    }
}
public class CategoryStat
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
    public double PercentageDecimal => Percentage / 100.0;
    public Color Color { get; set; } = Colors.Gray;
    public string DisplayAmount => Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"));
    public string DisplayPercentage => $"{Percentage:F1}%";
    public double BarWidth => Percentage * 2; // Scale for visual width
}