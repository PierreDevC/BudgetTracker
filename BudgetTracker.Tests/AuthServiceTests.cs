using BudgetTracker.Services;
using Xunit;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests unitaires pour AuthService.
/// </summary>
public class AuthServiceTests
{
    static async Task<(DatabaseService db, FakeUserSession session, AuthService auth)> CreateAsync()
    {
        // Un fichier par test : SQLiteConnectionPool partage les connexions ":memory:".
        var path = Path.Combine(Path.GetTempPath(), $"bt_test_{Guid.NewGuid():N}.db");
        var db = new DatabaseService(path, seedDemo: false);
        // Déclenche l'initialisation
        await db.GetUserByEmailAsync("init@trigger.com");

        var session = new FakeUserSession();
        var auth = new AuthService(db, session);
        return (db, session, auth);
    }

    static async Task<AuthService> RegisterUserAsync(AuthService auth,
        string name = "Alice", string email = "alice@example.com", string password = "motdepasse123")
    {
        await auth.RegisterAsync(name, email, password);
        return auth;
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_RetourneFaux_QuandUtilisateurInexistant()
    {
        var (_, _, auth) = await CreateAsync();

        var result = await auth.LoginAsync("inconnu@example.com", "motdepasse");

        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_RetourneFaux_QuandMauvaisMotDePasse()
    {
        var (_, _, auth) = await CreateAsync();
        await RegisterUserAsync(auth);

        var result = await auth.LoginAsync("alice@example.com", "mauvais_mdp");

        Assert.False(result);
    }

    [Fact]
    public async Task LoginAsync_RetourneVrai_QuandIdentifiantsCorrects()
    {
        var (_, _, auth) = await CreateAsync();
        await RegisterUserAsync(auth);

        var result = await auth.LoginAsync("alice@example.com", "motdepasse123");

        Assert.True(result);
    }

    [Fact]
    public async Task LoginAsync_SauvegardeSession_ApresSucces()
    {
        var (_, session, auth) = await CreateAsync();
        await RegisterUserAsync(auth);

        await auth.LoginAsync("alice@example.com", "motdepasse123");

        Assert.NotNull(session.GetSavedUserId());
    }

    [Fact]
    public async Task LoginAsync_NeSauvegardesPasSession_ApresEchec()
    {
        var (db, session, auth) = await CreateAsync();
        // On insère l'utilisateur directement pour ne pas déclencher la sauvegarde de session
        // que RegisterAsync effectue en cas de succès.
        await db.CreateUserAsync(new BudgetTracker.Models.User
        {
            Name = "Alice", Email = "alice@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("motdepasse123", 4)
        });

        await auth.LoginAsync("alice@example.com", "mauvais_mdp");

        Assert.Null(session.GetSavedUserId());
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_RetourneVrai_PourNouvelEmail()
    {
        var (_, _, auth) = await CreateAsync();

        var (success, error) = await auth.RegisterAsync("Bob", "bob@example.com", "monmotdepasse");

        Assert.True(success);
        Assert.Null(error);
    }

    [Fact]
    public async Task RegisterAsync_RetourneFaux_QuandEmailDejaUtilise()
    {
        var (_, _, auth) = await CreateAsync();
        await RegisterUserAsync(auth, email: "alice@example.com");

        var (success, error) = await auth.RegisterAsync("Alice2", "alice@example.com", "autremotdepasse");

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task RegisterAsync_HacheLeMotDePasse()
    {
        var (db, _, auth) = await CreateAsync();

        await auth.RegisterAsync("Bob", "bob@example.com", "monmotdepasse");
        var user = await db.GetUserByEmailAsync("bob@example.com");

        Assert.NotNull(user);
        Assert.NotEqual("monmotdepasse", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("monmotdepasse", user.PasswordHash));
    }

    // ── ChangePasswordAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_RetourneFaux_QuandAncienMdpIncorrect()
    {
        var (_, _, auth) = await CreateAsync();
        await RegisterUserAsync(auth);
        await auth.LoginAsync("alice@example.com", "motdepasse123");

        var (success, error) = await auth.ChangePasswordAsync("mauvais_mdp", "nouveaumdp123");

        Assert.False(success);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ChangePasswordAsync_RetourneVrai_EtMetAJourLeHash()
    {
        var (db, _, auth) = await CreateAsync();
        await RegisterUserAsync(auth);
        await auth.LoginAsync("alice@example.com", "motdepasse123");

        var (success, _) = await auth.ChangePasswordAsync("motdepasse123", "nouveaumdp456");
        var user = await db.GetUserByEmailAsync("alice@example.com");

        Assert.True(success);
        Assert.True(BCrypt.Net.BCrypt.Verify("nouveaumdp456", user!.PasswordHash));
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_EffaceSessionEtDonneesUtilisateur()
    {
        var (db, session, auth) = await CreateAsync();
        await RegisterUserAsync(auth);
        await auth.LoginAsync("alice@example.com", "motdepasse123");

        auth.Logout();

        Assert.Null(session.GetSavedUserId());
        Assert.Null(db.CurrentUser);
        Assert.Empty(db.Transactions);
    }

    // ── TryRestoreSessionAsync ────────────────────────────────────────────────

    [Fact]
    public async Task TryRestoreSessionAsync_RetourneFaux_SiAucuneSession()
    {
        var (_, _, auth) = await CreateAsync();

        var result = await auth.TryRestoreSessionAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task TryRestoreSessionAsync_RetourneVrai_EtRechargeUtilisateur()
    {
        var (db, session, auth) = await CreateAsync();
        await RegisterUserAsync(auth);
        await auth.LoginAsync("alice@example.com", "motdepasse123");

        // Simule une nouvelle instance d'AuthService avec la même session persistée
        var auth2 = new AuthService(db, session);
        var result = await auth2.TryRestoreSessionAsync();

        Assert.True(result);
        Assert.NotNull(db.CurrentUser);
        Assert.Equal("alice@example.com", db.CurrentUser.Email);
    }
}
