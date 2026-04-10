using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Models;
public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; } = TransactionType.Expense;

    public string DisplayAmount => Type == TransactionType.Income
        ? $"+{Amount:C}"
        : $"-{Amount:C}";

    public Color AmountColor => Type == TransactionType.Income
        ? Color.FromArgb("#00B894")
        : Color.FromArgb("#FF7675");
}

public enum TransactionType
{
    Income,
    Expense
}