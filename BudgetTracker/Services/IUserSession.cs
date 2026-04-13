namespace BudgetTracker.Services;

/// <summary>
/// Contrat pour la persistance de la session utilisateur.
/// Auteur: Pierre
/// </summary>
public interface IUserSession
{
    void Save(int userId);
    void Clear();
    int? GetSavedUserId();
}
