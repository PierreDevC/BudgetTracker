using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class LoginViewModel : BaseViewModel
{
    string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand DemoCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }
            // Simulated login — go to onboarding
            Application.Current!.MainPage = new NavigationPage(new Views.OnboardingPage());
        });

        DemoCommand = new Command(async () =>
        {
            var dataService = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<Services.MockDataService>();
            dataService.SeedDemoData();
            Application.Current.MainPage = new AppShell();
        });

        GoToRegisterCommand = new Command(async () =>
        {
            await Application.Current!.MainPage!.Navigation.PushAsync(new Views.RegisterPage());
        });
    }
}
