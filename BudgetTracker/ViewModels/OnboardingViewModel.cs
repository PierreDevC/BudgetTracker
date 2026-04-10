using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace BudgetTracker.ViewModels;

public class GoalOption
{
    public string Emoji { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Color IconBackground { get; set; } = Colors.LightGray;
}

public class OnboardingViewModel : BaseViewModel
{
    int _currentStep = 0;
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            SetProperty(ref _currentStep, value);
            OnPropertyChanged(nameof(IsStep0));
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(Dot0Opacity));
            OnPropertyChanged(nameof(Dot1Opacity));
            OnPropertyChanged(nameof(ShowNextButton));
            OnPropertyChanged(nameof(ShowSkipButton));
        }
    }

    public bool IsStep0 => CurrentStep == 0;
    public bool IsStep1 => CurrentStep == 1;

    public double Dot0Opacity => CurrentStep == 0 ? 1.0 : 0.3;
    public double Dot1Opacity => CurrentStep == 1 ? 1.0 : 0.3;

    // "Terminer" only shows on step 1; selecting a goal advances step 0 automatically
    public bool ShowNextButton => CurrentStep == 1;
    // "Passer" only on step 0
    public bool ShowSkipButton => CurrentStep == 0;

    public List<GoalOption> Goals { get; } = new()
    {
        new() { Emoji = "🏠", Name = "Acheter une maison",     Description = "Mise de fonds, frais de clôture, déménagement",  IconBackground = Color.FromArgb("#FEF0E7") },
        new() { Emoji = "🚀", Name = "Démarrer une entreprise", Description = "Capital de départ, équipement, inventaire",        IconBackground = Color.FromArgb("#E8F0FE") },
        new() { Emoji = "🛡️", Name = "Fonds d'urgence",         Description = "3-6 mois de dépenses pour la sécurité",           IconBackground = Color.FromArgb("#FDECEA") },
        new() { Emoji = "✈️", Name = "Voyage de rêve",          Description = "Vacances, lune de miel, aventure",                 IconBackground = Color.FromArgb("#E6F4EA") },
        new() { Emoji = "🎓", Name = "Éducation",               Description = "Diplôme, certification, formation",                IconBackground = Color.FromArgb("#F3E8FD") },
        new() { Emoji = "🚗", Name = "Acheter une voiture",     Description = "Filet de sécurité pour imprévus",                  IconBackground = Color.FromArgb("#EDE7F6") },
        new() { Emoji = "🎯", Name = "Autre objectif",          Description = "Définissez votre propre objectif",                 IconBackground = Color.FromArgb("#E8F5E9") },
    };

    GoalOption? _selectedGoal;
    public GoalOption? SelectedGoal
    {
        get => _selectedGoal;
        set => SetProperty(ref _selectedGoal, value);
    }

    string _monthlySavings = "";
    public string MonthlySavings { get => _monthlySavings; set => SetProperty(ref _monthlySavings, value); }

    string _monthsToGoal = "";
    public string MonthsToGoal { get => _monthsToGoal; set => SetProperty(ref _monthsToGoal, value); }

    string _incomeShare = "";
    public string IncomeShare { get => _incomeShare; set => SetProperty(ref _incomeShare, value); }

    public ICommand SelectGoalCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand SkipCommand { get; }

    public OnboardingViewModel()
    {
        SelectGoalCommand = new Command<GoalOption>(goal =>
        {
            SelectedGoal = goal;
            CurrentStep = 1;
        });

        NextCommand = new Command(() => FinishOnboarding());

        SkipCommand = new Command(() => FinishOnboarding());
    }

    void FinishOnboarding()
    {
        var dataService = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<Services.MockDataService>();
        dataService.SeedDemoData();

        if (SelectedGoal != null)
            dataService.CurrentUser.Goal = SelectedGoal.Name;

        Application.Current.MainPage = new AppShell();
    }
}
