using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour l'intégration de l'utilisateur.
/// Auteur : Pierre
/// </summary>
public class OnboardingViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

    /// <summary>
    /// Instance du fournisseur de services.
    /// </summary>
    readonly IServiceProvider _services;

    int _currentStep = 0;
    /// <summary>
    /// Obtient ou définit l'étape d'intégration actuelle.
    /// </summary>
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            SetProperty(ref _currentStep, value);
            OnPropertyChanged(nameof(IsStep0));
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            OnPropertyChanged(nameof(IsStep3));
            OnPropertyChanged(nameof(Dot0Opacity));
            OnPropertyChanged(nameof(Dot1Opacity));
            OnPropertyChanged(nameof(Dot2Opacity));
            OnPropertyChanged(nameof(Dot3Opacity));
            OnPropertyChanged(nameof(ShowNextButton));
            OnPropertyChanged(nameof(ShowFinishButton));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowSkipButton));
        }
    }

    /// <summary>
    /// Obtient une valeur indiquant s'il s'agit de l'étape 0.
    /// </summary>
    public bool IsStep0 => CurrentStep == 0;
    /// <summary>
    /// Obtient une valeur indiquant s'il s'agit de l'étape 1.
    /// </summary>
    public bool IsStep1 => CurrentStep == 1;
    /// <summary>
    /// Obtient une valeur indiquant s'il s'agit de l'étape 2.
    /// </summary>
    public bool IsStep2 => CurrentStep == 2;
    /// <summary>
    /// Obtient une valeur indiquant s'il s'agit de l'étape 3.
    /// </summary>
    public bool IsStep3 => CurrentStep == 3;

    /// <summary>
    /// Obtient l'opacité pour le point 0.
    /// </summary>
    public double Dot0Opacity => CurrentStep == 0 ? 1.0 : 0.3;
    /// <summary>
    /// Obtient l'opacité pour le point 1.
    /// </summary>
    public double Dot1Opacity => CurrentStep == 1 ? 1.0 : 0.3;
    /// <summary>
    /// Obtient l'opacité pour le point 2.
    /// </summary>
    public double Dot2Opacity => CurrentStep == 2 ? 1.0 : 0.3;
    /// <summary>
    /// Obtient l'opacité pour le point 3.
    /// </summary>
    public double Dot3Opacity => CurrentStep == 3 ? 1.0 : 0.3;

    /// <summary>
    /// Obtient une valeur indiquant s'il faut afficher le bouton Suivant.
    /// </summary>
    public bool ShowNextButton => CurrentStep < 3;
    /// <summary>
    /// Obtient une valeur indiquant s'il faut afficher le bouton Terminer.
    /// </summary>
    public bool ShowFinishButton => CurrentStep == 3;
    /// <summary>
    /// Obtient une valeur indiquant s'il faut afficher le bouton Retour.
    /// </summary>
    public bool ShowBackButton => CurrentStep > 0;
    /// <summary>
    /// Obtient une valeur indiquant s'il faut afficher le bouton Ignorer.
    /// </summary>
    public bool ShowSkipButton => CurrentStep == 0;

    string _monthlyIncome = "";
    /// <summary>
    /// Obtient ou définit le texte du revenu mensuel.
    /// </summary>
    public string MonthlyIncome { get => _monthlyIncome; set => SetProperty(ref _monthlyIncome, value); }

    string _groceriesBudget = "";
    /// <summary>
    /// Obtient ou définit le texte du budget épicerie.
    /// </summary>
    public string GroceriesBudget { get => _groceriesBudget; set => SetProperty(ref _groceriesBudget, value); }

    string _rentBudget = "";
    /// <summary>
    /// Obtient ou définit le texte du budget loyer.
    /// </summary>
    public string RentBudget { get => _rentBudget; set => SetProperty(ref _rentBudget, value); }

    string _transportBudget = "";
    /// <summary>
    /// Obtient ou définit le texte du budget transport.
    /// </summary>
    public string TransportBudget { get => _transportBudget; set => SetProperty(ref _transportBudget, value); }

    /// <summary>
    /// Commande pour passer à l'étape suivante.
    /// </summary>
    public ICommand NextCommand { get; }
    /// <summary>
    /// Commande pour revenir à l'étape précédente.
    /// </summary>
    public ICommand BackCommand { get; }
    /// <summary>
    /// Commande pour ignorer l'intégration.
    /// </summary>
    public ICommand SkipCommand { get; }
    /// <summary>
    /// Commande pour terminer l'intégration.
    /// </summary>
    public ICommand FinishCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de OnboardingViewModel.
    /// </summary>
    public OnboardingViewModel(Services.DatabaseService db, IServiceProvider services)
    {
        _db = db;
        _services = services;

        NextCommand = new Command(() => { if (CurrentStep < 3) CurrentStep++; });
        BackCommand = new Command(() => { if (CurrentStep > 0) CurrentStep--; });
        SkipCommand = new Command(async () => await FinishOnboardingAsync());
        FinishCommand = new Command(async () => await FinishOnboardingAsync());
    }

    /// <summary>
    /// Termine le processus d'intégration et sauvegarde les données.
    /// </summary>
    async Task FinishOnboardingAsync()
    {
        if (decimal.TryParse(MonthlyIncome, out var income))
        {
            _db.CurrentUser!.MonthlyIncome = income;
            await _db.UpdateUserAsync();
        }

        if (decimal.TryParse(GroceriesBudget, out var groceries))
            await _db.AddCategoryAsync(new Models.BudgetCategory { Name = "Épiceries", Amount = groceries });

        if (decimal.TryParse(RentBudget, out var rent))
            await _db.AddCategoryAsync(new Models.BudgetCategory { Name = "Loyer", Amount = rent });

        if (decimal.TryParse(TransportBudget, out var transport))
            await _db.AddCategoryAsync(new Models.BudgetCategory { Name = "Transport", Amount = transport });

        Application.Current!.MainPage = _services.GetRequiredService<AppShell>();
    }
}