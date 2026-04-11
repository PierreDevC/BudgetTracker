using System.Windows.Input;

namespace BudgetTracker.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    readonly Services.DatabaseService _db;
    readonly Services.AuthService _auth;
    readonly IServiceProvider _services;

    string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _currentPassword = string.Empty;
    public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

    string _newPassword = string.Empty;
    public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

    string _confirmNewPassword = string.Empty;
    public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

    public ICommand SaveCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand LogoutCommand { get; }

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
            _db.CurrentUser.Name = Name;
            await _db.UpdateUserAsync();
            await Shell.Current.DisplayAlert("Succès", "Profil mis à jour.", "OK");
        });

        ChangePasswordCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
                return;
            }
            if (NewPassword != ConfirmNewPassword)
            {
                await Shell.Current.DisplayAlert("Erreur", "Les nouveaux mots de passe ne correspondent pas.", "OK");
                return;
            }
            if (NewPassword.Length < 8)
            {
                await Shell.Current.DisplayAlert("Erreur", "Le mot de passe doit contenir au moins 8 caractères.", "OK");
                return;
            }
            var (success, error) = await _auth.ChangePasswordAsync(CurrentPassword, NewPassword);
            if (!success)
            {
                await Shell.Current.DisplayAlert("Erreur", error, "OK");
                return;
            }
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            await Shell.Current.DisplayAlert("Succès", "Mot de passe mis à jour.", "OK");
        });

        LogoutCommand = new Command(() =>
        {
            _auth.Logout();
            Application.Current!.MainPage = new NavigationPage(
                _services.GetRequiredService<Views.LoginPage>());
        });
    }
}
