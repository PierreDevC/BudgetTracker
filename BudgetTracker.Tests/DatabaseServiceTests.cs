using BudgetTracker.Models;
using BudgetTracker.Services;
using Xunit;

namespace BudgetTracker.Tests;

/// <summary>
/// Tests unitaires pour DatabaseService.
/// Chaque test utilise une base de données SQLite en mémoire isolée.
/// </summary>
public class DatabaseServiceTests
{
    // Crée une instance fraiche avec une base temporaire isolée et sans données de démonstration.
    // On utilise un fichier par test car SQLiteConnectionPool partage les connexions ":memory:".
    static async Task<DatabaseService> CreateDbAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bt_test_{Guid.NewGuid():N}.db");
        var db = new DatabaseService(path, seedDemo: false);
        // Déclenche l'initialisation (CreateTable, etc.)
        await db.GetUserByEmailAsync("init@trigger.com");
        return db;
    }

    static User MakeUser(string email = "test@example.com") => new()
    {
        Name = "Test",
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password", 4), // facteur 4 = rapide en test
        MonthlyIncome = 2000m
    };

    // ── GetUserByEmailAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsNull_QuandUtilisateurInexistant()
    {
        var db = await CreateDbAsync();

        var result = await db.GetUserByEmailAsync("inconnu@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_RetourneUtilisateur_ApresCreation()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser("alice@example.com"));

        var result = await db.GetUserByEmailAsync("alice@example.com");

        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("Test", result.Name);
    }

    // ── LoadUserDataAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task LoadUserDataAsync_RemplitCurrentUser()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");

        await db.LoadUserDataAsync(user!);

        Assert.NotNull(db.CurrentUser);
        Assert.Equal("test@example.com", db.CurrentUser.Email);
    }

    [Fact]
    public async Task LoadUserDataAsync_RemplitTransactionsEtCategories()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        await db.AddTransactionAsync(new Transaction
        {
            Name = "Salaire", Category = "Revenu", Amount = 1500m,
            Date = DateTime.Now, Type = TransactionType.Income
        });
        await db.AddCategoryAsync(new BudgetCategory { Name = "Transport", Amount = 200m });

        Assert.Single(db.Transactions);
        Assert.Single(db.BudgetCategories);
    }

    // ── AddTransactionAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddTransactionAsync_AssigneUserIdEtAjouteEnMemoire()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        await db.AddTransactionAsync(new Transaction
        {
            Name = "Épicerie", Category = "Alimentation", Amount = 80m,
            Date = DateTime.Now, Type = TransactionType.Expense
        });

        Assert.Single(db.Transactions);
        Assert.Equal(user!.Id, db.Transactions[0].UserId);
        Assert.Equal("Épicerie", db.Transactions[0].Name);
    }

    // ── TotalExpenses / Balance ───────────────────────────────────────────────

    [Fact]
    public async Task TotalExpenses_CompteSeulementDepensesDuMoisCourant()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        // Dépense ce mois-ci
        await db.AddTransactionAsync(new Transaction
        {
            Name = "Loyer", Category = "Logement", Amount = 1000m,
            Date = DateTime.Now, Type = TransactionType.Expense
        });
        // Dépense le mois précédent (ne doit pas être comptée)
        await db.AddTransactionAsync(new Transaction
        {
            Name = "Vieux loyer", Category = "Logement", Amount = 999m,
            Date = DateTime.Now.AddMonths(-1), Type = TransactionType.Expense
        });

        Assert.Equal(1000m, db.TotalExpenses);
    }

    [Fact]
    public async Task Balance_EstRevenuMensuelPlusRevenusMinusDepenses()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser()); // MonthlyIncome = 2000
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        await db.AddTransactionAsync(new Transaction
        {
            Name = "Bonus", Category = "Revenu", Amount = 500m,
            Date = DateTime.Now, Type = TransactionType.Income
        });
        await db.AddTransactionAsync(new Transaction
        {
            Name = "Loyer", Category = "Logement", Amount = 1200m,
            Date = DateTime.Now, Type = TransactionType.Expense
        });

        // Balance = 2000 (revenu mensuel) + 500 (revenu) - 1200 (dépense) = 1300
        Assert.Equal(1300m, db.Balance);
    }

    // ── DeleteCategoryAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCategoryAsync_SupprimeDeMemoire()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        await db.AddCategoryAsync(new BudgetCategory { Name = "Loisirs", Amount = 300m });
        var cat = db.BudgetCategories[0];

        await db.DeleteCategoryAsync(cat);

        Assert.Empty(db.BudgetCategories);
    }

    // ── ClearUserData ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ClearUserData_RemetToutAZero()
    {
        var db = await CreateDbAsync();
        await db.CreateUserAsync(MakeUser());
        var user = await db.GetUserByEmailAsync("test@example.com");
        await db.LoadUserDataAsync(user!);

        db.ClearUserData();

        Assert.Null(db.CurrentUser);
        Assert.Empty(db.Transactions);
        Assert.Empty(db.BudgetCategories);
    }
}
