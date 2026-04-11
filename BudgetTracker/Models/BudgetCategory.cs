using SQLite;

namespace BudgetTracker.Models;

[Table("BudgetCategories")]
public class BudgetCategory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
