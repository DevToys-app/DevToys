using System.Runtime.CompilerServices;

namespace DevToys.Api;

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetPropertyValue<T>(
        ref T field,
        T value,
        EventHandler? propertyChangedEventHandler,
        [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            propertyChangedEventHandler?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
