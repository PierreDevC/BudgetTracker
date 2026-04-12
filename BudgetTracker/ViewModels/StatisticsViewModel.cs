namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour afficher les statistiques.
/// Auteur : Aboubacar
/// </summary>
public class StatisticsViewModel : BaseViewModel
{
    /// <summary>
    /// Initialise une nouvelle instance de StatisticsViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    public StatisticsViewModel(Services.DatabaseService db)
    {
        Title = "Statistiques";
    }
}