using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Api;

namespace DevToys.Core.Tools.ViewItems;

/// <summary>
/// Represents a tool in the main menu.
/// </summary>
[DebuggerDisplay($"InternalComponentName = {{{nameof(ToolInstance)}.{nameof(GuiToolInstance.InternalComponentName)}}}")]
public sealed partial class GuiToolViewItem : ObservableObject
{
    private readonly bool _showLongDisplayTitle;

    public GuiToolViewItem(GuiToolInstance instance, bool showLongDisplayTitle = true)
    {
        Guard.IsNotNull(instance);
        ToolInstance = instance;
        _showLongDisplayTitle = showLongDisplayTitle;
    }

    /// <summary>
    /// Gets or sets the list of spans that matched a search.
    /// </summary>
    [ObservableProperty]
    private MatchSpan[]? _matchedSpans;

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
}
