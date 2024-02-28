namespace DevToys.Api;

/// <summary>
/// A component that can be used to display compared lists.
/// </summary>
public interface IUIDiffListInput : IUIMultiLineTextInput
{
    /// <summary>
    /// Gets whether the text diff control should case sensitive compared
    /// </summary>
    bool IsCaseSensitiveComparisonMode { get; }

    /// <summary>
    /// Gets the comparaisonMode of the element
    /// </summary>
    ListComparisonMode ComparaisonMode { get; }

    /// <summary>
    /// Raised when <see cref="IsCaseSensitiveComparisonMode"/> is changed.
    /// </summary>
    event EventHandler? CaseSensitiveModeChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal class UIDiffListInput : UIMultilineTextInput, IUIDiffListInput
{
    private bool _isCaseSensitiveComparison;
    private ListComparisonMode _comparaisonMode;
    internal UIDiffListInput(string? id)
        : base(id)
    {
    }

    public bool IsCaseSensitiveComparisonMode
    {
        get => _isCaseSensitiveComparison;
        internal set => SetPropertyValue(ref _isCaseSensitiveComparison, value, CaseSensitiveModeChanged);
    }

    public ListComparisonMode ComparaisonMode
    {
        get => _comparaisonMode;
        internal set => _comparaisonMode = value;
    }

    public event EventHandler? CaseSensitiveModeChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display compared lists.
    /// </summary>
    public static IUIDiffListInput DiffListInput()
    {
        return DiffListInput(null);
    }

    /// <summary>
    /// A component that can be used to display compared lists.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIDiffListInput DiffListInput(string? id)
    {
        return new UIDiffListInput(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIDiffListInput.ComparaisonMode"/> to AInterB.
    /// </summary>
    public static IUIDiffListInput CompareModeAInterB(this IUIDiffListInput element)
    {
        ((UIDiffListInput)element).ComparaisonMode = ListComparisonMode.AInterB;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDiffListInput.ComparaisonMode"/> to AOnly.
    /// </summary>
    public static IUIDiffListInput CompareModeAOnly(this IUIDiffListInput element)
    {
        ((UIDiffListInput)element).ComparaisonMode = ListComparisonMode.AOnly;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDiffListInput.ComparaisonMode"/> to BOnly.
    /// </summary>
    public static IUIDiffListInput CompareModeBOnly(this IUIDiffListInput element)
    {
        ((UIDiffListInput)element).ComparaisonMode = ListComparisonMode.BOnly;
        return element;
    }

    /// <summary>
    /// Indicates that the control can be extended to take the size of the whole tool boundaries.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    public static IUIDiffListInput Extendable(this IUIDiffListInput element)
    {
        ((UIDiffListInput)element).IsExtendableToFullScreen = true;
        return element;
    }

    /// <summary>
    /// Indicates that the control can not be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IUIDiffListInput NotExtendable(this IUIDiffListInput element)
    {
        ((UIDiffListInput)element).IsExtendableToFullScreen = false;
        return element;
    }
}
