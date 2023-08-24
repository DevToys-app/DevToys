namespace DevToys.Api;

/// <summary>
/// A component that splits horizontally or vertically an area into two panes. Panes can be resized by the user.
/// </summary>
public interface IUISplitGrid : IUIElementWithChildren
{
    /// <summary>
    /// Gets the orientation of the grid gutter.
    /// Default is <see cref="UIOrientation.Vertical"/>.
    /// </summary>
    UIOrientation Orientation { get; }

    /// <summary>
    /// Gets the minimum length of a cell, in pixels.
    /// Default is 50.
    /// </summary>
    int MinimumCellLength { get; }

    /// <summary>
    /// Gets the length of the top or left cell.
    /// Default is 1*. <see cref="UIGridLength.Auto"/> is not supported.
    /// </summary>
    UIGridLength LeftOrTopCellLength { get; }

    /// <summary>
    /// Gets the length of the right or bottom cell.
    /// Default is 1*. <see cref="UIGridLength.Auto"/> is not supported.
    /// </summary>
    UIGridLength RightOrBottomCellLength { get; }

    /// <summary>
    /// Gets the element to display in the right or bottom cell, depending on the <see cref="Orientation"/>.
    /// </summary>
    IUIElement? RightOrBottomCellContent { get; }

    /// <summary>
    /// Gets the element to display in the top or left cell, depending on the <see cref="Orientation"/>.
    /// </summary>
    IUIElement? LeftOrTopCellContent { get; }

    /// <summary>
    /// Raised when <see cref="Orientation"/> is changed.
    /// </summary>
    event EventHandler? OrientationChanged;

    /// <summary>
    /// Raised when <see cref="MinimumCellLength"/> is changed.
    /// </summary>
    event EventHandler? MinimumCellLengthChanged;

    /// <summary>
    /// Raised when <see cref="LeftOrTopCellLength"/> is changed.
    /// </summary>
    event EventHandler? LeftOrTopCellLengthChanged;

    /// <summary>
    /// Raised when <see cref="RightOrBottomCellLength"/> is changed.
    /// </summary>
    event EventHandler? RightOrBottomCellLengthChanged;

    /// <summary>
    /// Raised when <see cref="RightOrBottomCellContent"/> is changed.
    /// </summary>
    event EventHandler? RightOrBottomCellContentChanged;

    /// <summary>
    /// Raised when <see cref="LeftOrTopCellContent"/> is changed.
    /// </summary>
    event EventHandler? LeftOrTopCellContentChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Orientation = {{{nameof(Orientation)}}}")]
internal sealed class UISplitGrid : UIElementWithChildren, IUISplitGrid
{
    private UIOrientation _orientation = UIOrientation.Vertical;
    private int _minimumCellLength = 50;
    private UIGridLength _leftOrTopCellLength = new(1, UIGridUnitType.Fraction);
    private UIGridLength _rightOrBottomCellLength = new(1, UIGridUnitType.Fraction);
    private IUIElement? _rightOrBottomCellContent;
    private IUIElement? _leftOrTopCellContent;

    internal UISplitGrid(string? id)
        : base(id)
    {
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (_rightOrBottomCellContent is not null)
        {
            yield return _rightOrBottomCellContent;
        }

        if (_leftOrTopCellContent is not null)
        {
            yield return _leftOrTopCellContent;
        }
    }

    public UIOrientation Orientation
    {
        get => _orientation;
        internal set => SetPropertyValue(ref _orientation, value, OrientationChanged);
    }

    public int MinimumCellLength
    {
        get => _minimumCellLength;
        internal set => SetPropertyValue(ref _minimumCellLength, value, MinimumCellLengthChanged);
    }

    public UIGridLength LeftOrTopCellLength
    {
        get => _leftOrTopCellLength;
        internal set => SetPropertyValue(ref _leftOrTopCellLength, value, LeftOrTopCellLengthChanged);
    }

    public UIGridLength RightOrBottomCellLength
    {
        get => _rightOrBottomCellLength;
        internal set => SetPropertyValue(ref _rightOrBottomCellLength, value, RightOrBottomCellLengthChanged);
    }

    public IUIElement? RightOrBottomCellContent
    {
        get => _rightOrBottomCellContent;
        internal set => SetPropertyValue(ref _rightOrBottomCellContent, value, RightOrBottomCellContentChanged);
    }

    public IUIElement? LeftOrTopCellContent
    {
        get => _leftOrTopCellContent;
        internal set => SetPropertyValue(ref _leftOrTopCellContent, value, LeftOrTopCellContentChanged);
    }

    public event EventHandler? OrientationChanged;
    public event EventHandler? MinimumCellLengthChanged;
    public event EventHandler? LeftOrTopCellLengthChanged;
    public event EventHandler? RightOrBottomCellLengthChanged;
    public event EventHandler? RightOrBottomCellContentChanged;
    public event EventHandler? LeftOrTopCellContentChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that splits horizontally or vertically an area into two panes. Panes can be resized by the user.
    /// </summary>
    public static IUISplitGrid SplitGrid()
    {
        return SplitGrid(null);
    }

    /// <summary>
    /// Create a component that splits horizontally or vertically an area into two panes. Panes can be resized by the user.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISplitGrid SplitGrid(string? id)
    {
        return new UISplitGrid(id);
    }

    /// <summary>
    /// Split the <see cref="IUISplitGrid"/> vertically.
    /// </summary>
    public static IUISplitGrid Vertical(this IUISplitGrid element)
    {
        ((UISplitGrid)element).Orientation = UIOrientation.Vertical;
        return element;
    }

    /// <summary>
    /// Split the <see cref="IUISplitGrid"/> horizontally.
    /// </summary>
    public static IUISplitGrid Horizontal(this IUISplitGrid element)
    {
        ((UISplitGrid)element).Orientation = UIOrientation.Horizontal;
        return element;
    }

    /// <summary>
    /// Set the length of the top pane.
    /// </summary>
    public static IUISplitGrid TopPaneLength(this IUISplitGrid element, UIGridLength length)
    {
        GuardSplitGridLength(length);
        ((UISplitGrid)element).LeftOrTopCellLength = length;
        return element;
    }

    /// <summary>
    /// Set the length of the left pane.
    /// </summary>
    public static IUISplitGrid LeftPaneLength(this IUISplitGrid element, UIGridLength length)
    {
        GuardSplitGridLength(length);
        ((UISplitGrid)element).LeftOrTopCellLength = length;
        return element;
    }

    /// <summary>
    /// Set the length of the right pane.
    /// </summary>
    public static IUISplitGrid RightPaneLength(this IUISplitGrid element, UIGridLength length)
    {
        GuardSplitGridLength(length);
        ((UISplitGrid)element).RightOrBottomCellLength = length;
        return element;
    }

    /// <summary>
    /// Set the length of the bottom pane.
    /// </summary>
    public static IUISplitGrid BottomPaneLength(this IUISplitGrid element, UIGridLength length)
    {
        GuardSplitGridLength(length);
        ((UISplitGrid)element).RightOrBottomCellLength = length;
        return element;
    }

    /// <summary>
    /// Set the element to display in the top pane.
    /// </summary>
    public static IUISplitGrid WithTopPaneChild(this IUISplitGrid element, IUIElement? child)
    {
        ((UISplitGrid)element).LeftOrTopCellContent = child;
        return element;
    }

    /// <summary>
    /// Set the element to display in the left pane.
    /// </summary>
    public static IUISplitGrid WithLeftPaneChild(this IUISplitGrid element, IUIElement? child)
    {
        ((UISplitGrid)element).LeftOrTopCellContent = child;
        return element;
    }

    /// <summary>
    /// Set the element to display in the right pane.
    /// </summary>
    public static IUISplitGrid WithRightPaneChild(this IUISplitGrid element, IUIElement? child)
    {
        ((UISplitGrid)element).RightOrBottomCellContent = child;
        return element;
    }

    /// <summary>
    /// Set the element to display in the bottom pane.
    /// </summary>
    public static IUISplitGrid WithBottomPaneChild(this IUISplitGrid element, IUIElement? child)
    {
        ((UISplitGrid)element).RightOrBottomCellContent = child;
        return element;
    }

    /// <summary>
    /// Set the minimum length of a cell, in pixels.
    /// </summary>
    public static IUISplitGrid MinimumLength(this IUISplitGrid element, int minimumLength)
    {
        ((UISplitGrid)element).MinimumCellLength = minimumLength;
        return element;
    }

    private static void GuardSplitGridLength(UIGridLength length)
    {
        if (length.IsAuto)
        {
            ThrowHelper.ThrowArgumentException(nameof(length), $"{nameof(UIGridLength)}.{nameof(UIGridLength.Auto)} is not supported in {nameof(IUISplitGrid)}.");
        }
    }
}
