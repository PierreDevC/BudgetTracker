using System.Security.Cryptography;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class AuthService
{
    readonly DatabaseService _db;
    readonly UserSession _session;

    public AuthService(DatabaseService db, UserSession session)
    {
        _db = db;
        _session = session;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var id = _session.GetSavedUserId();
        if (id is null) return false;
        var user = await _db.GetUserByIdAsync(id.Value);
        if (user is null) return false;
        await _db.LoadUserDataAsync(user);
        return true;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _db.GetUserByEmailAsync(email.Trim().ToLower());
        if (user is null) return false;
        if (!Verify(password, user.PasswordHash, user.Salt)) return false;
        _session.Save(user.Id);
        await _db.LoadUserDataAsync(user);
        return true;
    }

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

    public void Logout()
    {
        _session.Clear();
        _db.ClearUserData();
    }

    static string GenerateSalt()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    static string Hash(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            Convert.FromBase64String(salt),
            100_000,
            HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    static bool Verify(string password, string hash, string salt)
        => Hash(password, salt) == hash;
}
