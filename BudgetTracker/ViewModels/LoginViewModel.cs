using System.Windows.Input;

namespace BudgetTracker.ViewModels;

public class LoginViewModel : BaseViewModel
{
    readonly Services.AuthService _auth;
    readonly IServiceProvider _services;

    string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(Services.AuthService auth, IServiceProvider services)
    {
        _auth = auth;
        _services = services;

        LoginCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }

            IsBusy = true;
            bool success = await _auth.LoginAsync(Email, Password);
            IsBusy = false;

            if (!success)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur", "Courriel ou mot de passe incorrect.", "OK");
                return;
            }

            Application.Current!.MainPage = _services.GetRequiredService<AppShell>();
        });

        GoToRegisterCommand = new Command(async () =>
        {
            await Application.Current!.MainPage!.Navigation.PushAsync(
                _services.GetRequiredService<Views.RegisterPage>());
        });
    }
}
