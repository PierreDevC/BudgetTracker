using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Fournit des services d'authentification et de gestion des utilisateurs.
/// Auteur: Pierre
/// </summary>
public class AuthService
{
    /// <summary>
    /// L'instance du service de base de données.
    /// </summary>
    readonly DatabaseService _db;

    /// <summary>
    /// L'instance de session utilisateur.
    /// </summary>
    readonly IUserSession _session;

    /// <summary>
    /// Le facteur de travail BCrypt (coût du hachage).
    /// </summary>
    const int WorkFactor = 12;

    /// <summary>
    /// Initialise une nouvelle instance de la classe AuthService.
    /// </summary>
    public AuthService(DatabaseService db, IUserSession session)
    {
        _db = db;
        _session = session;
    }

    /// <summary>
    /// Tente de restaurer la session utilisateur.
    /// </summary>
    /// <returns><c>true</c> si la session a été restaurée avec succès; sinon <c>false</c>.</returns>
    public async Task<bool> TryRestoreSessionAsync()
    {
        var id = _session.GetSavedUserId();
        if (id is null) return false;
        var user = await _db.GetUserByIdAsync(id.Value);
        if (user is null) return false;
        await _db.LoadUserDataAsync(user);
        return true;
    }

    /// <summary>
    /// Connecte un utilisateur avec e-mail et mot de passe.
    /// </summary>
    /// <param name="email">L'adresse e-mail de l'utilisateur.</param>
    /// <param name="password">Le mot de passe en clair.</param>
    /// <returns><c>true</c> si la connexion a réussi; sinon <c>false</c>.</returns>
    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _db.GetUserByEmailAsync(email.Trim().ToLower());
        if (user is null)
        {
            return false;
        }
        // BCrypt.Verify compare le mot de passe au hachage (sel extrait automatiquement)
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return false;
        }

        _session.Save(user.Id);
        await _db.LoadUserDataAsync(user);
        return true;
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur.
    /// </summary>
    /// <param name="name">Le nom complet de l'utilisateur.</param>
    /// <param name="email">L'adresse e-mail unique de l'utilisateur.</param>
    /// <param name="password">Le mot de passe en clair (au moins 8 caractères recommandé).</param>
    /// <returns>Un tuple contenant <c>success</c> (true si l'inscription a réussi) et <c>error</c> (message d'erreur ou null).</returns>
    public async Task<(bool success, string? error)> RegisterAsync(string name, string email, string password)
    {
        var existing = await _db.GetUserByEmailAsync(email.Trim().ToLower());
        if (existing is not null)
            return (false, "Un compte existe déjà avec ce courriel.");

        // BCrypt génère et intègre le sel dans la chaîne de hachage
        var hash = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        var user = new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = hash
        };
        await _db.CreateUserAsync(user);
        var created = await _db.GetUserByEmailAsync(user.Email);
        _session.Save(created!.Id);
        await _db.LoadUserDataAsync(created);
        return (true, null);
    }

    /// <summary>
    /// Modifie le mot de passe de l'utilisateur actuel.
    /// </summary>
    /// <param name="currentPassword">Le mot de passe actuel en clair (pour vérification).</param>
    /// <param name="newPassword">Le nouveau mot de passe en clair (au minimum 8 caractères).</param>
    /// <returns>Un tuple contenant <c>success</c> (true si le changement a réussi) et <c>error</c> (message d'erreur ou null).</returns>
    public async Task<(bool success, string? error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = _db.CurrentUser;
        if (user is null) return (false, "Aucun utilisateur connecté.");

        // Vérifie que le mot de passe actuel est correct avant d'autoriser le changement
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return (false, "Mot de passe actuel incorrect.");
        }

        // Génère un nouveau hachage BCrypt avec un nouveau sel intégré
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, WorkFactor);
        await _db.UpdateUserAsync();
        return (true, null);
    }

    /// <summary>
    /// Connecte directement le compte de démonstration.
    /// </summary>
    /// <returns><c>true</c> si la connexion a réussi.</returns>
    public Task<bool> LoginDemoAsync() => LoginAsync(DatabaseService.DemoEmail, "demo1234");

    /// <summary>
    /// Déconnecte l'utilisateur actuel.
    /// </summary>
    public void Logout()
    {
        _session.Clear();
        _db.ClearUserData();
    }
}
