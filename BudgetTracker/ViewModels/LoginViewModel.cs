using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour la connexion de l'utilisateur.
/// Auteur : Pierre
/// </summary>
public class LoginViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service d'authentification.
    /// </summary>
    readonly Services.AuthService _auth;

    /// <summary>
    /// Instance du fournisseur de services.
    /// </summary>
    readonly IServiceProvider _services;

    string _email = string.Empty;
    /// <summary>
    /// Obtient ou définit l'adresse e-mail.
    /// </summary>
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _password = string.Empty;
    /// <summary>
    /// Obtient ou définit le mot de passe.
    /// </summary>
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    /// <summary>
    /// Commande pour se connecter.
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Commande pour aller à la page d'inscription.
    /// </summary>
    public ICommand GoToRegisterCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de LoginViewModel.
    /// </summary>
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