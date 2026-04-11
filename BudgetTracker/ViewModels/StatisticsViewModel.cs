namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour afficher les statistiques.
/// Auteur : Pierre
/// </summary>
public class StatisticsViewModel : BaseViewModel
{
    /// <summary>
    /// Initialise une nouvelle instance de StatisticsViewModel.
    /// </summary>
    public StatisticsViewModel(Services.DatabaseService db)
    {
        Title = "Statistiques";
    }
}