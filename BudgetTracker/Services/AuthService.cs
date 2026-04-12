using System.Security.Cryptography;
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
    readonly UserSession _session;

    /// <summary>
    /// Initialise une nouvelle instance de la classe AuthService.
    /// </summary>
    public AuthService(DatabaseService db, UserSession session)
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
        if (user is null) return false;
        if (!Verify(password, user.PasswordHash, user.Salt)) return false;
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

        var salt = GenerateSalt();
        var hash = Hash(password, salt);
        var user = new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = hash,
            Salt = salt
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
        if (!Verify(currentPassword, user.PasswordHash, user.Salt))
            return (false, "Mot de passe actuel incorrect.");
        var newSalt = GenerateSalt();
        var newHash = Hash(newPassword, newSalt);
        user.PasswordHash = newHash;
        user.Salt = newSalt;
        await _db.UpdateUserAsync();
        return (true, null);
    }

    /// <summary>
    /// Déconnecte l'utilisateur actuel.
    /// </summary>
    public void Logout()
    {
        _session.Clear();
        _db.ClearUserData();
    }

    /// <summary>
    /// Génère un sel cryptographique aléatoire.
    /// </summary>
    /// <returns>Une chaîne Base64 représentant un sel de 16 octets.</returns>
    static string GenerateSalt()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Hache un mot de passe en utilisant PBKDF2 avec un sel donné.
    /// </summary>
    /// <param name="password">Le mot de passe en clair à hacher.</param>
    /// <param name="salt">Le sel en Base64 à utiliser pour le hachage (PBKDF2).</param>
    /// <returns>Une chaîne Base64 représentant le hachage PBKDF2 (SHA256, 100 000 itérations, 32 octets).</returns>
    static string Hash(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            Convert.FromBase64String(salt),
            100_000,
            HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    /// <summary>
    /// Vérifie un mot de passe par rapport à un hachage et un sel.
    /// </summary>
    /// <param name="password">Le mot de passe en clair à vérifier.</param>
    /// <param name="hash">Le hachage PBKDF2 stocké en Base64 pour la comparaison.</param>
    /// <param name="salt">Le sel en Base64 associé au hachage.</param>
    /// <returns><c>true</c> si le mot de passe correspond au hachage; sinon <c>false</c>.</returns>
    static bool Verify(string password, string hash, string salt)
        => Hash(password, salt) == hash;
}