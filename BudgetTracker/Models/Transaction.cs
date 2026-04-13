using SQLite;

namespace BudgetTracker.Models;

/// <summary>
/// Définit les types de transactions.
/// </summary>
public enum TransactionType { Income, Expense }

/// <summary>
/// Représente une transaction financière.
/// Auteur: Pierre
/// </summary>
[Table("Transactions")]
public class Transaction
{
    /// <summary>
    /// Obtient ou définit l'identifiant de la clé primaire.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Obtient ou définit l'ID de l'utilisateur associé.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Obtient ou définit le nom de la transaction.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit la catégorie de la transaction.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le montant de la transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Obtient ou définit la date de la transaction.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Obtient ou définit le type de la transaction.
    /// </summary>
    public TransactionType Type { get; set; } = TransactionType.Expense;

    /// <summary>
    /// Obtient le montant d'affichage formaté.
    /// </summary>
    [Ignore]
    public string DisplayAmount =>
        Type == TransactionType.Income
            ? $"+{Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}"
            : $"-{Amount.ToString("C", new System.Globalization.CultureInfo("fr-CA"))}";

    /// <summary>
    /// Obtient la couleur associée au montant de la transaction.
    /// </summary>
#if ANDROID || IOS || MACCATALYST || WINDOWS
    [Ignore]
    public Color AmountColor =>
        Type == TransactionType.Income
            ? Color.FromArgb("#00B894")
            : Color.FromArgb("#FF7675");
#endif
}