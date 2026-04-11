using SQLite;

namespace BudgetTracker.Models;

/// <summary>
/// Représente un utilisateur.
/// Auteur: Pierre
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Obtient ou définit l'identifiant de la clé primaire.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Obtient ou définit l'adresse e-mail unique.
    /// </summary>
    [Unique]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le nom de l'utilisateur.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le mot de passe haché.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le sel du mot de passe.
    /// </summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le revenu mensuel de l'utilisateur.
    /// </summary>
    public decimal MonthlyIncome { get; set; }
}