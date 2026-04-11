using BudgetTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Services;
public class MockDataService
{
    public User CurrentUser { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();
    public decimal MonthlyIncome { get; set; } = 0m;
    public List<BudgetCategory> BudgetCategories { get; set; } = new();

    public void SeedDemoData()
    {
        CurrentUser = new User
        {
            Name = "Démo Utilisateur",
            Email = "demo@budgettracker.ca",
            Goal = "Économiser pour un voyage",
            Reason = "Mieux gérer mes finances"
        };

        Transactions = new List<Transaction>();

        MonthlyIncome = 3200.00m;

        BudgetCategories = new List<BudgetCategory>
        {
            new() { Name = "Loyer",           Amount = 950 },
            new() { Name = "Épiceries",       Amount = 300 },
            new() { Name = "Transport",       Amount = 90  },
            new() { Name = "Téléphone",       Amount = 55  },
            new() { Name = "Internet",        Amount = 65  },
            new() { Name = "Utilitaires",     Amount = 80  },
            new() { Name = "CELI",            Amount = 200 },
            new() { Name = "Diners",          Amount = 120 },
            new() { Name = "Abonnements",     Amount = 50  },
            new() { Name = "Divertissement",  Amount = 80  },
        };
    }

    // Computed summaries for Home page
    public decimal TotalExpenses => Transactions
        .Where(t => t.Type == TransactionType.Expense && t.Date.Month == DateTime.Now.Month)
        .Sum(t => t.Amount);

    public decimal TotalIncome => Transactions
        .Where(t => t.Type == TransactionType.Income && t.Date.Month == DateTime.Now.Month)
        .Sum(t => t.Amount);

    public decimal Balance => MonthlyIncome - TotalExpenses;

    public int TransactionCount => Transactions
        .Count(t => t.Date.Month == DateTime.Now.Month);
}
