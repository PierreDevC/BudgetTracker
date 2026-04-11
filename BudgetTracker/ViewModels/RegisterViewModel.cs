using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour l'inscription de l'utilisateur.
/// Auteur : Pierre
/// </summary>
public class RegisterViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service d'authentification.
    /// </summary>
    readonly Services.AuthService _auth;

    /// <summary>
    /// Instance du fournisseur de services.
    /// </summary>
    readonly IServiceProvider _services;

    string _name = string.Empty;
    /// <summary>
    /// Obtient ou définit le nom de l'utilisateur.
    /// </summary>
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _email = string.Empty;
    /// <summary>
    /// Obtient ou définit l'e-mail de l'utilisateur.
    /// </summary>
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _password = string.Empty;
    /// <summary>
    /// Obtient ou définit le mot de passe de l'utilisateur.
    /// </summary>
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    string _confirmPassword = string.Empty;
    /// <summary>
    /// Obtient ou définit le mot de passe de confirmation de l'utilisateur.
    /// </summary>
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    /// <summary>
    /// Commande pour inscrire l'utilisateur.
    /// </summary>
    public ICommand RegisterCommand { get; }

    /// <summary>
    /// Commande pour retourner à la connexion.
    /// </summary>
    public ICommand BackToLoginCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de RegisterViewModel.
    /// </summary>
    public RegisterViewModel(Services.AuthService auth, IServiceProvider services)
    {
        _auth = auth;
        _services = services;

        RegisterCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }
            if (Password != ConfirmPassword)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erreur", "Les mots de passe ne correspondent pas.", "OK");
                return;
            }

            IsBusy = true;
            var (success, error) = await _auth.RegisterAsync(Name, Email, Password);
            IsBusy = false;

            if (!success)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erreur", error!, "OK");
                return;
            }

            Application.Current!.MainPage = new NavigationPage(
                _services.GetRequiredService<Views.OnboardingPage>());
        });

        BackToLoginCommand = new Command(async () =>
        {
            await Application.Current!.MainPage!.Navigation.PopAsync();
        });
    }
}