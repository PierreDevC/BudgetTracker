namespace BudgetTracker.Services;

/// <summary>
/// Gère les données de session de l'utilisateur.
/// Auteur: Pierre
/// </summary>
public class UserSession : IUserSession
{
    /// <summary>
    /// La clé de préférence pour l'ID de l'utilisateur actuel.
    /// </summary>
    const string Key = "current_user_id";

    /// <summary>
    /// Enregistre l'ID de l'utilisateur dans les préférences.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur à enregistrer.</param>
#if ANDROID || IOS || MACCATALYST || WINDOWS
    public void Save(int userId) => Preferences.Set(Key, userId);
#else
    public void Save(int userId) { }
#endif

    /// <summary>
    /// Efface l'ID de l'utilisateur enregistré des préférences.
    /// </summary>
#if ANDROID || IOS || MACCATALYST || WINDOWS
    public void Clear() => Preferences.Remove(Key);
#else
    public void Clear() { }
#endif

    /// <summary>
    /// Récupère l'ID de l'utilisateur enregistré depuis les préférences.
    /// </summary>
    /// <returns>L'identifiant de l'utilisateur, ou <c>null</c> si aucun utilisateur n'est enregistré.</returns>
#if ANDROID || IOS || MACCATALYST || WINDOWS
    public int? GetSavedUserId() =>
        Preferences.ContainsKey(Key) ? Preferences.Get(Key, 0) : null;
#else
    public int? GetSavedUserId() => null;
#endif
}