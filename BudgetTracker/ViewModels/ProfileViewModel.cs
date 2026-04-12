using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;

/// <summary>
/// ViewModel pour la gestion du profil utilisateur.
/// Auteur : Pierre
/// </summary>
public class ProfileViewModel : BaseViewModel
{
    /// <summary>
    /// Instance du service de base de données.
    /// </summary>
    readonly Services.DatabaseService _db;

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

    string _currentPassword = string.Empty;
    /// <summary>
    /// Obtient ou définit le mot de passe actuel.
    /// </summary>
    public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

    string _newPassword = string.Empty;
    /// <summary>
    /// Obtient ou définit le nouveau mot de passe.
    /// </summary>
    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

    string _confirmNewPassword = string.Empty;
    /// <summary>
    /// Obtient ou définit la confirmation du nouveau mot de passe.
    /// </summary>
    public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

    /// <summary>
    /// Commande pour sauvegarder le profil.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Commande pour changer le mot de passe.
    /// </summary>
    public ICommand ChangePasswordCommand { get; }

    /// <summary>
    /// Commande pour se déconnecter.
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// Initialise une nouvelle instance de ProfileViewModel.
    /// </summary>
    /// <param name="db">Le service de base de données.</param>
    /// <param name="auth">Le service d'authentification.</param>
    /// <param name="services">Le fournisseur de services pour la navigation.</param>
    public ProfileViewModel(Services.DatabaseService db, Services.AuthService auth, IServiceProvider services)
    {
        _db = db;
        _auth = auth;
        _services = services;
        Title = "Profil";

        Name  = _db.CurrentUser?.Name  ?? string.Empty;
        Email = _db.CurrentUser?.Email ?? string.Empty;

        SaveCommand = new Command(async () =>
        {
            if (_db.CurrentUser is null) return;
            // Met à jour le nom de l'utilisateur dans l'objet en mémoire
            _db.CurrentUser.Name = Name;
            // Enregistre les changements dans la base de données
            await _db.UpdateUserAsync();
#if !WINDOWS
            await Toast.Make("Profil mis à jour.", ToastDuration.Short).Show();
#endif
        });

        ChangePasswordCommand = new Command(async () =>
        {
            // Valide que tous les champs sont remplis
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }

            // Valide que le nouveau mot de passe et sa confirmation correspondent
            if (NewPassword != ConfirmNewPassword)
            {
                await Shell.Current.DisplayAlert("Erreur", "Les nouveaux mots de passe ne correspondent pas.", "OK");
                return;
            }

            // Valide la longueur minimale du nouveau mot de passe
            if (NewPassword.Length < 8)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le mot de passe doit contenir au moins 8 caractères.", "OK");
                return;
            }

            // Appelle le service d'authentification pour changer le mot de passe
            var (success, error) = await _auth.ChangePasswordAsync(CurrentPassword, NewPassword);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Erreur", error, "OK");
                return;
            }

            // Vide les champs de saisie après succès
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
#if !WINDOWS
            await Toast.Make("Mot de passe mis à jour.", ToastDuration.Short).Show();
#endif
        });

        LogoutCommand = new Command(() =>
        {
            // Efface les données de session et d'utilisateur
            _auth.Logout();
            // Navigue vers la page de connexion
            Application.Current!.MainPage = new NavigationPage(
                _services.GetRequiredService<Views.LoginPage>());
        });
    }
}