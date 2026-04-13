using BudgetTracker.Services;

namespace BudgetTracker.Tests;

/// <summary>
/// Fausse session utilisateur pour les tests (remplace MAUI Preferences).
/// </summary>
public class FakeUserSession : IUserSession
{
    int? _userId;

    public void Save(int userId) => _userId = userId;
    public void Clear() => _userId = null;
    public int? GetSavedUserId() => _userId;
}
