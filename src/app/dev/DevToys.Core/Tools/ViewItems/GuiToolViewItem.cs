using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToys.Core.Tools.ViewItems;

/// <summary>
/// Represents a tool in the main menu.
/// </summary>
[DebuggerDisplay($"InternalComponentName = {{{nameof(ToolInstance)}.{nameof(GuiToolInstance.InternalComponentName)}}}")]
public sealed partial class GuiToolViewItem : ObservableObject, IItem
{
    private readonly bool _showLongDisplayTitle;

    public GuiToolViewItem(GuiToolInstance instance, bool showLongDisplayTitle = true, TextSpan[]? matchSpans = null)
    {
        Guard.IsNotNull(instance);
        ToolInstance = instance;
        MatchedSpans = matchSpans;
        _showLongDisplayTitle = showLongDisplayTitle;
    }

    /// <summary>
    /// Raised when the tool got selected in the menu.
    /// </summary>
    public event EventHandler? GotSelected;

    /// <summary>
    /// Gets or sets the list of spans that matched a search.
    /// </summary>
    [ObservableProperty]
    private TextSpan[]? _matchedSpans;

    /// <summary>
    /// Gets or sets whether the tool is recommended to be used (for example, after detecting a compatible data from Smart Detection).
    /// </summary>
    [ObservableProperty]
    private bool _isRecommended;

    /// <summary>
    /// Gets the instance of the tool.
    /// </summary>
    public GuiToolInstance ToolInstance { get; }

    public string MenuDisplayTitle
    {
        get
        {
            if (!_showLongDisplayTitle || string.IsNullOrWhiteSpace(ToolInstance.LongDisplayTitle))
            {
                return ToolInstance.ShortDisplayTitle;
            }

            return ToolInstance.LongDisplayTitle;
        }
    }

    public void RaiseGotSelected()
    {
        GotSelected?.Invoke(this, EventArgs.Empty);
    }
}
