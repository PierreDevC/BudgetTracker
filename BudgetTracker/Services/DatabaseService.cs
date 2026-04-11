using SQLite;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class DatabaseService
{
    SQLiteAsyncConnection? _db;

    // ── In-memory state (populated from DB on login) ─────────────────────────────
    public User? CurrentUser { get; private set; }
    public List<Transaction> Transactions { get; private set; } = new();
    public List<BudgetCategory> BudgetCategories { get; private set; } = new();

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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        await InitAsync();
        await _db!.InsertAsync(user);
    }

    public async Task UpdateUserAsync()
    {
        if (CurrentUser is null) return;
        await InitAsync();
        await _db!.UpdateAsync(CurrentUser);
    }

    // ── Session ───────────────────────────────────────────────────────────────────

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

    public void ClearUserData()
    {
        CurrentUser = null;
        Transactions.Clear();
        BudgetCategories.Clear();
    }

    // ── Computed (sync, from in-memory) ───────────────────────────────────────────

    public decimal TotalExpenses => Transactions
        .Where(t => t.Type == TransactionType.Expense &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    public decimal TotalIncome => Transactions
        .Where(t => t.Type == TransactionType.Income &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

    public decimal Balance => (CurrentUser?.MonthlyIncome ?? 0) + TotalIncome - TotalExpenses;

    public int TransactionCount => Transactions
        .Count(t => t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year);

    // ── Transactions ──────────────────────────────────────────────────────────────

    public async Task AddTransactionAsync(Transaction t)
    {
        await InitAsync();
        t.UserId = CurrentUser!.Id;
        await _db!.InsertAsync(t);
        Transactions.Insert(0, t);
    }

    // ── Budget Categories ─────────────────────────────────────────────────────────

    public async Task AddCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        cat.UserId = CurrentUser!.Id;
        await _db!.InsertAsync(cat);
        BudgetCategories.Add(cat);
    }

    public async Task UpdateCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.UpdateAsync(cat);
    }

    public async Task DeleteCategoryAsync(BudgetCategory cat)
    {
        await InitAsync();
        await _db!.DeleteAsync(cat);
        BudgetCategories.Remove(cat);
    }
}
