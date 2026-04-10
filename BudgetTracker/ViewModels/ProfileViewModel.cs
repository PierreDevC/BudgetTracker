using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class ProfileViewModel : BaseViewModel
{
    readonly Services.MockDataService _data;

    string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    string _goal = string.Empty;
    public string Goal { get => _goal; set => SetProperty(ref _goal, value); }

    string _reason = string.Empty;
    public string Reason { get => _reason; set => SetProperty(ref _reason, value); }

    public ICommand SaveCommand { get; }
    public ICommand LogoutCommand { get; }

    public ProfileViewModel(Services.MockDataService data)
    {
        _data = data;
        Title = "Profil";

        Name = _data.CurrentUser.Name;
        Email = _data.CurrentUser.Email;
        Goal = _data.CurrentUser.Goal;
        Reason = _data.CurrentUser.Reason;

        SaveCommand = new Command(async () =>
        {
            _data.CurrentUser.Name = Name;
            _data.CurrentUser.Email = Email;
            _data.CurrentUser.Goal = Goal;
            _data.CurrentUser.Reason = Reason;
            await Shell.Current.DisplayAlert("Succès", "Profil mis à jour.", "OK");
        });

        LogoutCommand = new Command(() =>
        {
            Application.Current!.MainPage = new NavigationPage(new Views.LoginPage());
        });
    }
}
