using SQLite;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

/// <summary>
/// Fournit des opérations de base de données locales.
/// Auteur: Pierre
/// </summary>
public class DatabaseService
{
    /// <summary>
    /// L'instance de connexion SQLite.
    /// </summary>
    SQLiteAsyncConnection? _db;

    // ── In-memory state (populated from DB on login) ─────────────────────────────

    /// <summary>
    /// Obtient l'utilisateur actuellement connecté.
    /// </summary>
    public User? CurrentUser { get; private set; }

    /// <summary>
    /// Obtient la liste des transactions de l'utilisateur actuel.
    /// </summary>
    public List<Transaction> Transactions { get; private set; } = new();

    /// <summary>
    /// Obtient la liste des catégories de budget de l'utilisateur actuel.
    /// </summary>
    public List<BudgetCategory> BudgetCategories { get; private set; } = new();

    /// <summary>
    /// Initialise la connexion à la base de données et les tables.
    /// </summary>
    async Task InitAsync()
    {
        if (_db is not null) return;
        var path = Path.Combine(FileSystem.AppDataDirectory, "budget.db");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<User>();
        await _db.CreateTableAsync<Transaction>();
        await _db.CreateTableAsync<BudgetCategory>();
    }

    // ── User ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Récupère un utilisateur par son e-mail.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Récupère un utilisateur par son ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Crée un nouvel utilisateur dans la base de données.
    /// </summary>
    public async Task CreateUserAsync(User user)
    {
        await InitAsync();
        await _db!.InsertAsync(user);
    }

    /// <summary>
    /// Met à jour les informations de l'utilisateur actuel.
    /// </summary>
    public async Task UpdateUserAsync()
    {
        if (CurrentUser is null) return;
        await InitAsync();
        await _db!.UpdateAsync(CurrentUser);
    }

    // ── Session ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Charge les données de l'utilisateur en mémoire.
    /// </summary>
    public async Task LoadUserDataAsync(User user)
    {
        await InitAsync();
        CurrentUser = user;
        Transactions = (await _db!.Table<Transaction>()
            .Where(t => t.UserId == user.Id)
            .ToListAsync())
            .OrderByDescending(t => t.Date)
            .ToList();
        BudgetCategories = await _db!.Table<BudgetCategory>()
            .Where(c => c.UserId == user.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Efface les données de l'utilisateur de la mémoire.
    /// </summary>
    public void ClearUserData()
    {
        CurrentUser = null;
        Transactions.Clear();
        BudgetCategories.Clear();
    }

    // ── Computed (sync, from in-memory) ───────────────────────────────────────────

    /// <summary>
    /// Obtient le total des dépenses pour le mois en cours.
    /// </summary>
    public decimal TotalExpenses => Transactions
        .Where(t => t.Type == TransactionType.Expense &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    /// <summary>
    /// Obtient le revenu total pour le mois en cours.
    /// </summary>
    public decimal TotalIncome => Transactions
        .Where(t => t.Type == TransactionType.Income &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    /// <summary>
    /// Obtient le solde actuel de l'utilisateur.
    /// </summary>
    public decimal Balance => (CurrentUser?.MonthlyIncome ?? 0) + TotalIncome - TotalExpenses;

    /// <summary>
    /// Obtient le nombre de transactions pour le mois en cours.
    /// </summary>
    public int TransactionCount => Transactions
        .Count(t => t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year);

    // ── Transactions ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Ajoute une nouvelle transaction à la base de données et à la mémoire.
    /// </summary>
    public async Task AddTransactionAsync(Transaction t)
    {
        await InitAsync();
        t.UserId = CurrentUser!.Id;
        await _db!.InsertAsync(t);
        Transactions.Insert(0, t);
    }

    // ── Budget Categories ─────────────────────────────────────────────────────────

    /// <summary>
    /// Ajoute une nouvelle catégorie de budget à la base de données et à la mémoire.
    /// </summary>
    public async Task AddCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        cat.UserId = CurrentUser!.Id;
        await _db!.InsertAsync(cat);
        BudgetCategories.Add(cat);
    }

    /// <summary>
    /// Met à jour une catégorie de budget existante dans la base de données.
    /// </summary>
    public async Task UpdateCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.UpdateAsync(cat);
    }

    /// <summary>
    /// Supprime une catégorie de budget de la base de données et de la mémoire.
    /// </summary>
    public async Task DeleteCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.DeleteAsync(cat);
        BudgetCategories.Remove(cat);
    }
}