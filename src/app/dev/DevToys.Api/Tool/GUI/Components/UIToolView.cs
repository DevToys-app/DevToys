using System.Runtime.CompilerServices;

namespace DevToys.Api;

/// <summary>
/// Represents the root ui definition of a <see cref="IGuiTool"/>.
/// </summary>
[DebuggerDisplay($"RootElement = {{{nameof(RootElement)}}}")]
public class UIToolView : INotifyPropertyChanged
{
    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="rootElement">The root element of the tool's UI.</param>
    public UIToolView(IUIElement rootElement)
    {
        Guard.IsNotNull(rootElement);
        RootElement = rootElement;
    }

    /// <summary>
    /// Gets the root element of the tool's UI.
    /// </summary>
    public IUIElement RootElement { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetPropertyValue<T>(
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
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}
