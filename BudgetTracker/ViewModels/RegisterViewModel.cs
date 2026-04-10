using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class RegisterViewModel : BaseViewModel
{
    string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    string _confirmPassword = string.Empty;
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    public RegisterViewModel()
    {
        RegisterCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }
            if (Password != ConfirmPassword)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }
            Application.Current!.MainPage = new NavigationPage(new Views.OnboardingPage());
        });

        BackToLoginCommand = new Command(async () =>
        {
            await Application.Current!.MainPage!.Navigation.PopAsync();
        });
    }
}
