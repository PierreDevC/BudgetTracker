using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BudgetTracker.ViewModels;

/// <summary>
/// Classe de base pour tous les view models.
/// Auteur : Pierre
/// </summary>
public class BaseViewModel : INotifyPropertyChanged
{
    bool _isBusy;
    /// <summary>
    /// Obtient ou définit une valeur indiquant si le view model est occupé.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    string _title = string.Empty;
    /// <summary>
    /// Obtient ou définit le titre du view model.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Événement déclenché lorsque la valeur d'une propriété change.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Définit la valeur de la propriété et déclenche l'événement PropertyChanged si nécessaire.
    /// </summary>
    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Déclenche l'événement PropertyChanged.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}