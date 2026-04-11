using SQLite;

namespace BudgetTracker.Models;

/// <summary>
/// Représente une catégorie de budget pour un utilisateur.
/// Auteur: Pierre
/// </summary>
[Table("BudgetCategories")]
public class BudgetCategory
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
    /// Obtient ou définit le nom de la catégorie.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le montant alloué.
    /// </summary>
    public decimal Amount { get; set; }
}