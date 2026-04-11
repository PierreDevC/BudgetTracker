using SQLite;

namespace BudgetTracker.Models;

public enum TransactionType { Income, Expense }

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Ignore]
    public string DisplayAmount =>
        Type == TransactionType.Income
            ? $"+{Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}"
            : $"-{Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}";

    [Ignore]
    public Color AmountColor =>
        Type == TransactionType.Income
            ? Color.FromArgb("#00B894")
            : Color.FromArgb("#FF7675");
}
