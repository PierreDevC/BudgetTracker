using Foundation;
using BudgetTracker.Platforms.iOS.Handlers;

namespace BudgetTracker
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            EntryHandlerCustomization.RemoveNativeBorder();
            return MauiProgram.CreateMauiApp();
        }
    }
}
