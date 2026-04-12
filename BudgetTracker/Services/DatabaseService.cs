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

    // ── State de mémorire (ajouté à partir de la base de données) ─────────────────────────────

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
    /// Adresse e-mail du compte de démonstration.
    /// </summary>
    public const string DemoEmail = "demo@budgettracker.app";

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
        await SeedDemoAccountAsync();
    }

    /// <summary>
    /// Crée le compte démo avec des catégories et transactions pré-remplies, une seule fois.
    /// </summary>
    async Task SeedDemoAccountAsync()
    {
        var existing = await _db!.Table<User>().Where(u => u.Email == DemoEmail).FirstOrDefaultAsync();
        if (existing is not null) return;

        var demo = new User
        {
            Name = "Démo",
            Email = DemoEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo1234", 12),
            MonthlyIncome = 3500
        };
        await _db!.InsertAsync(demo);
        var user = await _db!.Table<User>().Where(u => u.Email == DemoEmail).FirstOrDefaultAsync();
        int uid = user!.Id;

        // Catégories
        var categories = new[]
        {
            new BudgetCategory { UserId = uid, Name = "Alimentation",  Amount = 500  },
            new BudgetCategory { UserId = uid, Name = "Transport",      Amount = 200  },
            new BudgetCategory { UserId = uid, Name = "Logement",       Amount = 1200 },
            new BudgetCategory { UserId = uid, Name = "Loisirs",        Amount = 300  },
            new BudgetCategory { UserId = uid, Name = "Santé",          Amount = 150  },
            new BudgetCategory { UserId = uid, Name = "Vêtements",      Amount = 200  },
            new BudgetCategory { UserId = uid, Name = "Restaurant",     Amount = 250  },
            new BudgetCategory { UserId = uid, Name = "Épargne",        Amount = 400  },
        };
        foreach (var c in categories)
            await _db!.InsertAsync(c);

        // Dates relatives au mois courant et au mois précédent
        var now = DateTime.Now;
        var cur = (int y, int m, int d) => new DateTime(y, m, d);
        int cy = now.Year, cm = now.Month;
        int py = cm == 1 ? cy - 1 : cy;
        int pm = cm == 1 ? 12 : cm - 1;

        var transactions = new[]
        {
            // ── Mois courant — revenus ──────────────────────────────────────────
            new Transaction { UserId=uid, Name="Salaire",          Category="Revenu",       Amount=2800m,  Date=cur(cy,cm,1),  Type=TransactionType.Income  },
            new Transaction { UserId=uid, Name="Contrat pigiste",  Category="Revenu",       Amount=700m,   Date=cur(cy,cm,5),  Type=TransactionType.Income  },

            // ── Mois courant — dépenses ─────────────────────────────────────────
            new Transaction { UserId=uid, Name="Loyer",            Category="Logement",     Amount=1200m,  Date=cur(cy,cm,1),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Virement REER",    Category="Épargne",      Amount=200m,   Date=cur(cy,cm,1),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Passe STM",        Category="Transport",    Amount=94m,    Date=cur(cy,cm,2),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Netflix",          Category="Loisirs",      Amount=17m,    Date=cur(cy,cm,3),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Spotify",          Category="Loisirs",      Amount=10.99m, Date=cur(cy,cm,3),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Épicerie IGA",     Category="Alimentation", Amount=87.50m, Date=cur(cy,cm,3),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Essence",          Category="Transport",    Amount=68m,    Date=cur(cy,cm,4),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Manteau d'hiver",  Category="Vêtements",    Amount=139m,   Date=cur(cy,cm,5),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Pharmacie",        Category="Santé",        Amount=42.50m, Date=cur(cy,cm,6),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Resto Thaï",       Category="Restaurant",   Amount=48m,    Date=cur(cy,cm,7),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Épicerie Metro",   Category="Alimentation", Amount=65.20m, Date=cur(cy,cm,8),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Sushi Mikado",     Category="Restaurant",   Amount=62m,    Date=cur(cy,cm,9),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Cinéma",           Category="Loisirs",      Amount=32m,    Date=cur(cy,cm,10), Type=TransactionType.Expense },

            // ── Mois précédent — revenus ────────────────────────────────────────
            new Transaction { UserId=uid, Name="Salaire",          Category="Revenu",       Amount=2800m,  Date=cur(py,pm,1),  Type=TransactionType.Income  },
            new Transaction { UserId=uid, Name="Remboursement",    Category="Revenu",       Amount=120m,   Date=cur(py,pm,15), Type=TransactionType.Income  },

            // ── Mois précédent — dépenses ───────────────────────────────────────
            new Transaction { UserId=uid, Name="Loyer",            Category="Logement",     Amount=1200m,  Date=cur(py,pm,1),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Virement REER",    Category="Épargne",      Amount=200m,   Date=cur(py,pm,1),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Gym Énergie",      Category="Santé",        Amount=45m,    Date=cur(py,pm,3),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Épicerie IGA",     Category="Alimentation", Amount=92m,    Date=cur(py,pm,5),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Amazon",           Category="Loisirs",      Amount=54.99m, Date=cur(py,pm,8),  Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Essence",          Category="Transport",    Amount=71m,    Date=cur(py,pm,10), Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Lunch bureau",     Category="Restaurant",   Amount=28.50m, Date=cur(py,pm,12), Type=TransactionType.Expense },
            new Transaction { UserId=uid, Name="Chaussures sport", Category="Vêtements",    Amount=89m,    Date=cur(py,pm,18), Type=TransactionType.Expense },
        };
        foreach (var t in transactions)
            await _db!.InsertAsync(t);
    }

    // ── User ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Récupère un utilisateur par son e-mail.
    /// </summary>
    /// <param name="email">L'adresse e-mail de l'utilisateur à rechercher.</param>
    /// <returns>L'utilisateur trouvé, ou <c>null</c> si aucun utilisateur ne correspond.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Récupère un utilisateur par son ID.
    /// </summary>
    /// <param name="id">L'identifiant unique de l'utilisateur.</param>
    /// <returns>L'utilisateur trouvé, ou <c>null</c> si aucun utilisateur ne correspond.</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Crée un nouvel utilisateur dans la base de données.
    /// </summary>
    /// <param name="user">L'objet utilisateur contenant les informations à insérer.</param>
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
    /// Charge les données de l'utilisateur en mémoire (transactions et catégories).
    /// </summary>
    /// <param name="user">L'utilisateur dont charger les données.</param>
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

    // ── Dépenses, revenu, solde (synchronisation, de la mémoire) ───────────────────────────────────────────

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
    /// Ajoute une nouvelle transaction à la base de données et à la mémoire (au début de la liste).
    /// </summary>
    /// <param name="t">L'objet transaction à ajouter.</param>
    public async Task AddTransactionAsync(Transaction t)
    {
        await InitAsync();
        t.UserId = CurrentUser!.Id;
        await _db!.InsertAsync(t);
        Transactions.Insert(0, t);
    }

    // ── Categories de budget ─────────────────────────────────────────────────────────

    /// <summary>
    /// Ajoute une nouvelle catégorie de budget à la base de données et à la mémoire.
    /// </summary>
    /// <param name="cat">L'objet catégorie de budget à ajouter.</param>
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
    /// <param name="cat">L'objet catégorie de budget modifié à mettre à jour.</param>
    public async Task UpdateCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.UpdateAsync(cat);
    }

    /// <summary>
    /// Supprime une catégorie de budget de la base de données et de la mémoire.
    /// </summary>
    /// <param name="cat">L'objet catégorie de budget à supprimer.</param>
    public async Task DeleteCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.DeleteAsync(cat);
        BudgetCategories.Remove(cat);
    }
}