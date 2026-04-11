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
    static string GenerateSalt()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Hache un mot de passe en utilisant PBKDF2 avec un sel donné.
    /// </summary>
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
    static bool Verify(string password, string hash, string salt)
        => Hash(password, salt) == hash;
}