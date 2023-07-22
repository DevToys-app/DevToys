using System.Runtime.CompilerServices;

namespace DevToys.Api;

/// <summary>
/// Represents the root ui definition of a <see cref="IGuiTool"/>.
/// </summary>
[DebuggerDisplay($"RootElement = {{{nameof(RootElement)}}}, IsScrollable = {{{nameof(IsScrollable)}}}")]
public class UIToolView : INotifyPropertyChanged
{
    private bool _isScrollable;

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="rootElement">The root element of the tool's UI.</param>
    public UIToolView(IUIElement rootElement)
        : this(true, rootElement)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="isScrollable">Indicates whether the UI of the tool is scrollable or should fits to the window boundaries.</param>
    /// <param name="rootElement">The root element of the tool's UI.</param>
    public UIToolView(bool isScrollable, IUIElement rootElement)
    {
        IsScrollable = isScrollable;
        RootElement = rootElement;
    }

    /// <summary>
    /// Gets whether the UI of the tool is scrollable or should fits to the window boundaries. Default is true.
    /// </summary>
    public bool IsScrollable
    {
        get => _isScrollable;
        internal set => SetPropertyValue(ref _isScrollable, value, IsScrollableChanged);
    }

    /// <summary>
    /// Gets the root element of the tool's UI.
    /// </summary>
    public IUIElement RootElement { get; }

    public event EventHandler? IsScrollableChanged;

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
