using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BudgetTracker.ViewModels;
public class SplashViewModel : BaseViewModel    
{
    public ICommand NavigateCommand { get; }

    public SplashViewModel()
    {
        NavigateCommand = new Command(async () =>
        {
            await Task.Delay(2500);
            Application.Current!.MainPage = new NavigationPage(new Views.LoginPage());
        });
    }
}
