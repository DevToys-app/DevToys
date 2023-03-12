namespace DevToys.Api;

/// <summary>
/// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
/// </summary>
public interface IUISetting : IUITitledElement
{
    /// <summary>
    /// Gets the description of the setting.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the icon of the setting.
    /// </summary>
    IUIIcon? Icon { get; }

    /// <summary>
    /// Gets the <see cref="IUIElement"/> that represents the interactive part of the setting.
    /// </summary>
    IUIElement? InteractiveElement { get; }

    /// <summary>
    /// Raised when <see cref="Description"/> is changed.
    /// </summary>
    public event EventHandler? DescriptionChanged;

    /// <summary>
    /// Raised when <see cref="Icon"/> is changed.
    /// </summary>
    public event EventHandler? IconChanged;

    /// <summary>
    /// Raised when <see cref="InteractiveElement"/> is changed.
    /// </summary>
    public event EventHandler? InteractiveElementChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal class UISetting : UITitledElement, IUISetting
{
    private string? _description;
    private IUIIcon? _icon;
    private IUIElement? _interactiveElement;

    internal UISetting(string? id)
        : base(id)
    {
    }

    public string? Description
    {
        get => _description;
        internal set
        {
            _description = value;
            DescriptionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IUIIcon? Icon
    {
        get => _icon;
        internal set
        {
            _icon = value;
            IconChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IUIElement? InteractiveElement
    {
        get => _interactiveElement;
        internal set
        {
            _interactiveElement = value;
            InteractiveElementChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DescriptionChanged;
    public event EventHandler? IconChanged;
    public event EventHandler? InteractiveElementChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    public static IUISetting Setting()
    {
        return Setting(null);
    }

    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISetting Setting(string? id)
    {
        return new UISetting(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Description"/> of the setting.
    /// </summary>
    public static IUISetting Description(this IUISetting element, string? text)
    {
        ((UISetting)element).Description = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Icon"/> of the setting.
    /// </summary>
    public static IUISetting Icon(this IUISetting element, string fontName, string glyph)
    {
        ((UISetting)element).Icon = Icon(fontName, glyph);
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.InteractiveElement"/> of the setting.
    /// </summary>
    public static IUISetting InteractiveElement(this IUISetting element, IUIElement? uiElement)
    {
        ((UISetting)element).InteractiveElement = uiElement;
        return element;
    }
}
