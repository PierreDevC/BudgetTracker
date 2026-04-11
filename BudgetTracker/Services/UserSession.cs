namespace BudgetTracker.Services;

/// <summary>
/// Gère les données de session de l'utilisateur.
/// Auteur: Pierre
/// </summary>
public class UserSession
{
    /// <summary>
    /// La clé de préférence pour l'ID de l'utilisateur actuel.
    /// </summary>
    const string Key = "current_user_id";

    /// <summary>
    /// Enregistre l'ID de l'utilisateur dans les préférences.
    /// </summary>
    public void Save(int userId) => Preferences.Set(Key, userId);

    /// <summary>
    /// Efface l'ID de l'utilisateur enregistré des préférences.
    /// </summary>
    public void Clear() => Preferences.Remove(Key);

    /// <summary>
    /// Récupère l'ID de l'utilisateur enregistré depuis les préférences.
    /// </summary>
    public int? GetSavedUserId() =>
        Preferences.ContainsKey(Key) ? Preferences.Get(Key, 0) : null;
}