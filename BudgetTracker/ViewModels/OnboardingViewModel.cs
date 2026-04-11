using System.Windows.Input;

namespace BudgetTracker.ViewModels;

public class OnboardingViewModel : BaseViewModel
{
    readonly Services.DatabaseService _db;
    readonly IServiceProvider _services;

    int _currentStep = 0;
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

    public bool IsStep0 => CurrentStep == 0;
    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;

    public double Dot0Opacity => CurrentStep == 0 ? 1.0 : 0.3;
    public double Dot1Opacity => CurrentStep == 1 ? 1.0 : 0.3;
    public double Dot2Opacity => CurrentStep == 2 ? 1.0 : 0.3;
    public double Dot3Opacity => CurrentStep == 3 ? 1.0 : 0.3;

    public bool ShowNextButton => CurrentStep < 3;
    public bool ShowFinishButton => CurrentStep == 3;
    public bool ShowBackButton => CurrentStep > 0;
    public bool ShowSkipButton => CurrentStep == 0;

    string _monthlyIncome = "";
    public string MonthlyIncome { get => _monthlyIncome; set => SetProperty(ref _monthlyIncome, value); }

    string _groceriesBudget = "";
    public string GroceriesBudget { get => _groceriesBudget; set => SetProperty(ref _groceriesBudget, value); }

    string _rentBudget = "";
    public string RentBudget { get => _rentBudget; set => SetProperty(ref _rentBudget, value); }

    string _transportBudget = "";
    public string TransportBudget { get => _transportBudget; set => SetProperty(ref _transportBudget, value); }

    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand FinishCommand { get; }

    public OnboardingViewModel(Services.DatabaseService db, IServiceProvider services)
    {
        _db = db;
        _services = services;

        NextCommand = new Command(() => { if (CurrentStep < 3) CurrentStep++; });
        BackCommand = new Command(() => { if (CurrentStep > 0) CurrentStep--; });
        SkipCommand = new Command(async () => await FinishOnboardingAsync());
        FinishCommand = new Command(async () => await FinishOnboardingAsync());
    }

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
