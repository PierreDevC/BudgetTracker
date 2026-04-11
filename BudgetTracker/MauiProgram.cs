using MauiIcons.Fluent.Filled;
using Microsoft.Extensions.Logging;

namespace BudgetTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFluentFilledMauiIcons()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Merriweather-Bold.ttf", "MerriweatherBold");
                fonts.AddFont("Merriweather-Regular.ttf", "MerriweatherRegular");
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

        // Services
        builder.Services.AddSingleton<Services.MockDataService>();

        // ViewModels
        builder.Services.AddTransient<ViewModels.SplashViewModel>();
        builder.Services.AddTransient<ViewModels.LoginViewModel>();
        builder.Services.AddTransient<ViewModels.RegisterViewModel>();
        builder.Services.AddTransient<ViewModels.OnboardingViewModel>();
        builder.Services.AddTransient<ViewModels.HomeViewModel>();
        builder.Services.AddTransient<ViewModels.BudgetViewModel>();
        builder.Services.AddTransient<ViewModels.TransactionsViewModel>();
        builder.Services.AddTransient<ViewModels.StatisticsViewModel>();
        builder.Services.AddTransient<ViewModels.ProfileViewModel>();
        builder.Services.AddTransient<ViewModels.AddExpenseViewModel>();
        builder.Services.AddTransient<ViewModels.AddRevenuViewModel>();

        // Pages
        builder.Services.AddTransient<Views.SplashPage>();
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<Views.RegisterPage>();
        builder.Services.AddTransient<Views.OnboardingPage>();
        builder.Services.AddTransient<Views.HomePage>();
        builder.Services.AddTransient<Views.BudgetPage>();
        builder.Services.AddTransient<Views.TransactionsPage>();
        builder.Services.AddTransient<Views.StatisticsPage>();
        builder.Services.AddTransient<Views.ProfilePage>();
        builder.Services.AddTransient<Views.AddExpensePage>();
        builder.Services.AddTransient<Views.AddRevenuPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}