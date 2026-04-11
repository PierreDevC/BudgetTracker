namespace BudgetTracker.Services;

public class UserSession
{
    const string Key = "current_user_id";

    public void Save(int userId) => Preferences.Set(Key, userId);
    public void Clear() => Preferences.Remove(Key);

    public int? GetSavedUserId() =>
        Preferences.ContainsKey(Key) ? Preferences.Get(Key, 0) : null;
}
