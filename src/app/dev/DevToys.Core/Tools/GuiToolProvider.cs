using System.Collections.ObjectModel;
using System.Reflection;
using DevToys.Api;
using DevToys.Core.Settings;
using DevToys.Core.Tools.Metadata;
using DevToys.Core.Tools.ViewItems;
using DevToys.Localization.Strings.MainMenu;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Core.Tools;

/// <summary>
/// Provides information about tools in the app.
/// </summary>
[Export(typeof(GuiToolProvider))]
public sealed partial class GuiToolProvider
{
    private readonly ILogger _logger;
    private readonly IEnumerable<Lazy<IResourceManagerAssemblyIdentifier, ResourceManagerAssemblyIdentifierMetadata>> _resourceManagerAssemblyIdentifiers;
    private readonly IEnumerable<Lazy<GuiToolGroup, GuiToolGroupMetadata>> _guiToolGroups;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IReadOnlyList<GuiToolInstance> _footerToolInstances;
    private readonly IReadOnlyList<GuiToolInstance> _bodyToolInstances;

    private ObservableCollection<INotifyPropertyChanged>? _headerAndBodyToolViewItems;
    private ReadOnlyObservableCollection<INotifyPropertyChanged>? _headerAndBodyToolViewItemsReadOnly;
    private ObservableCollection<GuiToolViewItem>? _footerToolViewItems;
    private ReadOnlyObservableCollection<GuiToolViewItem>? _footerToolViewItemsReadOnly;
    private GroupViewItem? _favoriteToolsGroupViewItem;
    private SeparatorViewItem? _separatorAfterAllToolsItem;
    private SeparatorViewItem? _separatorAfterRecentTools;

    [ImportingConstructor]
    public GuiToolProvider(
        [ImportMany] IEnumerable<Lazy<IGuiTool, GuiToolMetadata>> guiTools,
        [ImportMany] IEnumerable<Lazy<GuiToolGroup, GuiToolGroupMetadata>> guiToolGroups,
        [ImportMany] IEnumerable<Lazy<IResourceManagerAssemblyIdentifier, ResourceManagerAssemblyIdentifierMetadata>> resourceManagerAssemblyIdentifiers,
        ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _resourceManagerAssemblyIdentifiers = resourceManagerAssemblyIdentifiers;
        _settingsProvider = settingsProvider;
        _guiToolGroups = guiToolGroups;

        BuildGuiToolInstances(
            guiTools,
            out IReadOnlyList<GuiToolInstance>? allTools,
            out _footerToolInstances,
            out _bodyToolInstances);

        AllTools = allTools;
    }

    /// <summary>
    /// Gets a flat list containing all the tools available, ordered.
    /// </summary>
    public IReadOnlyList<GuiToolInstance> AllTools { get; }

    /// <summary>
    /// Gets a hierarchical list containing all the tools available, ordered, to display in the top and body menu.
    /// This includes "All tools" menu item, recents and favorites.
    /// </summary>
    public ReadOnlyObservableCollection<INotifyPropertyChanged> HeaderAndBodyToolViewItems
    {
        get
        {
            if (_headerAndBodyToolViewItems is null)
            {
                Guard.IsNull(_headerAndBodyToolViewItemsReadOnly);
                _headerAndBodyToolViewItems = new(BuildHeaderAndBodyToolViewItems());
                _headerAndBodyToolViewItemsReadOnly = new(_headerAndBodyToolViewItems);
            }

            Guard.IsNotNull(_headerAndBodyToolViewItemsReadOnly);
            return _headerAndBodyToolViewItemsReadOnly;
        }
    }

    /// <summary>
    /// Gets a flat list containing all the footer tools available, ordered.
    /// </summary>
    public ReadOnlyObservableCollection<GuiToolViewItem> FooterToolViewItems
    {
        get
        {
            if (_footerToolViewItems is null)
            {
                Guard.IsNull(_footerToolViewItemsReadOnly);
                _footerToolViewItems = new(BuildFooterToolViewItems());
                _footerToolViewItemsReadOnly = new(_footerToolViewItems);
            }

            Guard.IsNotNull(_footerToolViewItemsReadOnly);
            return _footerToolViewItemsReadOnly;
        }
    }

    /// <summary>
    /// Sets a tool as the most recently used one.
    /// </summary>
    public void SetMostRecentUsedTool(GuiToolInstance guiToolInstance)
    {
        Guard.IsNotNull(guiToolInstance);
        _settingsProvider.SetSetting(PredefinedSettings.RecentTool3, _settingsProvider.GetSetting(PredefinedSettings.RecentTool2));
        _settingsProvider.SetSetting(PredefinedSettings.RecentTool2, _settingsProvider.GetSetting(PredefinedSettings.RecentTool1));
        _settingsProvider.SetSetting(PredefinedSettings.RecentTool1, guiToolInstance.InternalComponentName);

        LogSetMostRecentUsedTool(guiToolInstance.InternalComponentName);
    }

    /// <summary>
    /// Sets whether the given tool is marked as favorite or not.
    /// </summary>
    public void SetToolIsFavorite(GuiToolInstance guiToolInstance, bool isFavorite)
    {
        SettingDefinition<bool> isFavoriteSettingDefinition = CreateIsToolFavoriteSettingDefinition(guiToolInstance);
        _settingsProvider.SetSetting(isFavoriteSettingDefinition, isFavorite);

        // Update the list in the menu.
        if (_headerAndBodyToolViewItems is not null)
        {
            Guard.HasSizeGreaterThan((ICollection<INotifyPropertyChanged>)_headerAndBodyToolViewItems!, 2);
            Guard.IsOfType<SeparatorViewItem>(_headerAndBodyToolViewItems[1]);

            if (isFavorite) // Add the tool to the favorites
            {
                if (_favoriteToolsGroupViewItem is null)
                {
                    var newFavoriteToolViewItem = new GuiToolViewItem(guiToolInstance);
                    CreateFavoriteGroupViewItem(new[] { newFavoriteToolViewItem });
                    Guard.IsNotNull(_favoriteToolsGroupViewItem);
                }
                else
                {
                    Guard.IsNotNull(_favoriteToolsGroupViewItem.Children);
                    var newFavoriteToolViewItem = new GuiToolViewItem(guiToolInstance);
                    _favoriteToolsGroupViewItem.Children!.Insert(0, newFavoriteToolViewItem);
                }

                // If the "Favorites" menu item isn't present in the menu, add it after the Recent tools (if any).
                if (!_headerAndBodyToolViewItems.Contains(_favoriteToolsGroupViewItem))
                {
                    int separatorIndex = -1;
                    if (_separatorAfterRecentTools is not null)
                    {
                        separatorIndex = _headerAndBodyToolViewItems.IndexOf(_separatorAfterRecentTools);
                    }
                    else
                    {
                        Guard.IsNotNull(_separatorAfterAllToolsItem);
                        separatorIndex = _headerAndBodyToolViewItems.IndexOf(_separatorAfterAllToolsItem);
                    }

                    _headerAndBodyToolViewItems.Insert(separatorIndex + 1, _favoriteToolsGroupViewItem);
                }
            }
            else // Remove the tool to the favorites
            {
                if (_favoriteToolsGroupViewItem is not null)
                {
                    Guard.IsNotNull(_favoriteToolsGroupViewItem.Children);
                    GuiToolViewItem? favoriteToolToRemove = _favoriteToolsGroupViewItem.Children.FirstOrDefault(t => t.ToolInstance == guiToolInstance);
                    if (favoriteToolToRemove is not null)
                    {
                        _favoriteToolsGroupViewItem.Children.Remove(favoriteToolToRemove);
                    }

                    // If the "Favorites" menu item is empty, remove it from the menu.
                    if (_favoriteToolsGroupViewItem.Children.Count == 0)
                    {
                        _headerAndBodyToolViewItems.Remove(_favoriteToolsGroupViewItem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets whether the given tool is favorite or not.
    /// </summary>
    public bool GetToolIsFavorite(GuiToolInstance guiToolInstance)
    {
        Guard.IsNotNull(guiToolInstance);
        SettingDefinition<bool> isFavoriteSettingDefinition = CreateIsToolFavoriteSettingDefinition(guiToolInstance);
        return _settingsProvider.GetSetting(isFavoriteSettingDefinition);
    }

    /// <summary>
    /// Gets all the <see cref="GuiToolViewItem"/> the given <paramref name="guiToolInstance"/> appears in.
    /// </summary>
    public IEnumerable<GuiToolViewItem> GetViewItemFromTool(GuiToolInstance guiToolInstance)
    {
        for (int i = 0; i < FooterToolViewItems.Count; i++)
        {
            if (FooterToolViewItems[i].ToolInstance == guiToolInstance)
            {
                yield return FooterToolViewItems[i];
            }
        }

        for (int i = 0; i < HeaderAndBodyToolViewItems.Count; i++)
        {
            INotifyPropertyChanged item = HeaderAndBodyToolViewItems[i];
            if (item is GuiToolViewItem guiToolViewItem && guiToolViewItem.ToolInstance == guiToolInstance)
            {
                yield return guiToolViewItem;
            }
            else if (item is GroupViewItem groupViewItem && groupViewItem.Children is not null)
            {
                for (int j = 0; j < groupViewItem.Children.Count; j++)
                {
                    INotifyPropertyChanged subItem = groupViewItem.Children[j];
                    if (subItem is GuiToolViewItem subGuiToolViewItem && subGuiToolViewItem.ToolInstance == guiToolInstance)
                    {
                        yield return subGuiToolViewItem;
                    }
                }
            }
        }
    }

    private void BuildGuiToolInstances(
        IEnumerable<Lazy<IGuiTool, GuiToolMetadata>> guiTools,
        out IReadOnlyList<GuiToolInstance> allTools,
        out IReadOnlyList<GuiToolInstance> footerTools,
        out IReadOnlyList<GuiToolInstance> bodyTools)
    {
        var allToolInstances = new List<GuiToolInstance>();
        var footerToolInstances = new List<GuiToolInstance>();
        var bodyToolInstances = new List<GuiToolInstance>();

        // Order all the tools.
        guiTools = ExtensionOrderer.Order(guiTools);

        foreach (Lazy<IGuiTool, GuiToolMetadata> guiToolDefinition in guiTools)
        {
            // Make sure the tool is supported by the current OS. If no platform is precised by the tool,
            // it means it's supported by every OS.
            if (!OSHelper.IsOsSupported(guiToolDefinition.Metadata.TargetPlatforms))
            {
                Debug.WriteLine($"Ignoring '{guiToolDefinition.Metadata.InternalComponentName}' tool as it isn't supported by the current OS.");
                continue;
            }

            // if possible, get the resource manager for the tool.
            Assembly? resourceManagerAssembly = null;
            if (!string.IsNullOrEmpty(guiToolDefinition.Metadata.ResourceManagerAssemblyIdentifier))
            {
                resourceManagerAssembly = GetResourceManagerAssembly(guiToolDefinition.Metadata.ResourceManagerAssemblyIdentifier);
            }

            // Create the tool instance and store it in a menu placement list.
            var instance = new GuiToolInstance(guiToolDefinition, resourceManagerAssembly);
            allToolInstances.Add(instance);

            switch (guiToolDefinition.Metadata.MenuPlacement)
            {
                case null:
                case MenuPlacement.Body:
                    bodyToolInstances.Add(instance);
                    break;

                case MenuPlacement.Footer:
                    footerToolInstances.Add(instance);
                    break;

                default:
                    ThrowHelper.ThrowNotSupportedException();
                    break;
            }
        }

        allTools = allToolInstances;
        footerTools = footerToolInstances;
        bodyTools = bodyToolInstances;
    }

    private IEnumerable<GuiToolViewItem> BuildFooterToolViewItems()
    {
        foreach (GuiToolInstance item in _footerToolInstances)
        {
            yield return new GuiToolViewItem(item);
        }
    }

    /// <summary>
    /// Creates the list of menu items to display at the top in the navigation view.
    /// </summary>
    /// <remarks>
    /// It generates the following pattern:
    /// - All Tools
    /// - ---------
    /// - Recent tool 1
    /// - Recent tool 2
    /// - Recent tool 3
    /// - ---------
    /// - Favorites (expanded by default when the app starts)
    ///   | - Tool 1
    ///   | - Tool 2
    /// - Converters
    /// - Encoders / Decoders
    /// - ...etc
    /// </remarks>
    /// <returns></returns>
    private IEnumerable<INotifyPropertyChanged> BuildHeaderAndBodyToolViewItems()
    {
        // "All tools" menu item.
        yield return new GroupViewItem(
            iconFontName: "FluentSystemIcons",
            iconGlyph: "\uE70F",
            displayTitle: MainMenu.AllToolsDisplayTitle,
            accessibleName: MainMenu.AllToolsAccessibleName);

        // Separator.
        _separatorAfterAllToolsItem = new SeparatorViewItem();
        yield return _separatorAfterAllToolsItem;

        // Recent tools.
        foreach (INotifyPropertyChanged recentToolItem in BuildRecentToolViewItems())
        {
            yield return recentToolItem;
        }

        // Favorites.
        foreach (INotifyPropertyChanged favoriteToolItem in BuildFavoriteToolViewItems())
        {
            yield return favoriteToolItem;
        }

        // All other tools
        foreach (INotifyPropertyChanged groupAndChildren in BuildAllToolsTreeViewItems())
        {
            yield return groupAndChildren;
        }
    }

    private IEnumerable<INotifyPropertyChanged> BuildRecentToolViewItems()
    {
        GuiToolInstance? recentTool1 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool1));
        GuiToolInstance? recentTool2 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool2));
        GuiToolInstance? recentTool3 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool3));
        if (recentTool1 is not null || recentTool2 is not null || recentTool3 is not null)
        {
            if (recentTool1 is not null)
            {
                yield return new GuiToolViewItem(recentTool1);
            }

            if (recentTool2 is not null)
            {
                yield return new GuiToolViewItem(recentTool2);
            }

            if (recentTool3 is not null)
            {
                yield return new GuiToolViewItem(recentTool3);
            }

            // Separator.
            _separatorAfterRecentTools = new SeparatorViewItem();
            yield return _separatorAfterRecentTools;
        }
    }

    private IEnumerable<INotifyPropertyChanged> BuildFavoriteToolViewItems()
    {
        var favoriteTools = new List<GuiToolViewItem>();
        for (int i = 0; i < AllTools.Count; i++)
        {
            GuiToolInstance tool = AllTools[i];
            bool isFavorite = GetToolIsFavorite(tool);
            if (isFavorite)
            {
                favoriteTools.Add(new GuiToolViewItem(tool));
            }
        }

        if (favoriteTools.Count > 0)
        {
            // Create favorite group.
            yield return CreateFavoriteGroupViewItem(favoriteTools);
        }
    }

    private IEnumerable<INotifyPropertyChanged> BuildAllToolsTreeViewItems()
    {
        var groups = new Dictionary<string, Lazy<GroupViewItem, GuiToolGroupMetadata>>();

        // For each tool
        for (int i = 0; i < _bodyToolInstances.Count; i++)
        {
            GuiToolInstance tool = _bodyToolInstances[i];

            // Find the group associated to the tool.
            Lazy<GuiToolGroup, GuiToolGroupMetadata>? toolGroup = _guiToolGroups.FirstOrDefault(g => string.Equals(g.Metadata.InternalComponentName, tool.GroupName, StringComparison.Ordinal));

            if (toolGroup is null)
            {
                ThrowHelper.ThrowInvalidDataException($"Unable to find the group named '{tool.GroupName}' that the tool '{tool.InternalComponentName}' is pointing to.");
                return null;
            }

            // Create a group view presentation, if needed.
            if (!groups.TryGetValue(tool.GroupName, out Lazy<GroupViewItem, GuiToolGroupMetadata>? groupViewItem))
            {
                groupViewItem = new(() => new GroupViewItem(toolGroup.Value, new ObservableCollection<GuiToolViewItem>()), toolGroup.Metadata);
                groups[tool.GroupName] = groupViewItem;
            }

            // Add the tool to the group.
            Guard.IsNotNull(groupViewItem.Value.Children);
            groupViewItem.Value.Children.Add(new GuiToolViewItem(tool));
        }

        // Order tools groups.
        IEnumerable<GroupViewItem> orderedGroups = ExtensionOrderer.Order(groups.Values).Select(g => g.Value);
        return orderedGroups;
    }

    private Assembly GetResourceManagerAssembly(string resourceManagerAssemblyIdentifier)
    {
        foreach (Lazy<IResourceManagerAssemblyIdentifier, ResourceManagerAssemblyIdentifierMetadata> item in _resourceManagerAssemblyIdentifiers)
        {
            if (string.Equals(item.Metadata.InternalComponentName, resourceManagerAssemblyIdentifier, StringComparison.Ordinal))
            {
                return item.Value.GetType().Assembly;
            }
        }

        throw new InvalidDataException($"Unable to find the {nameof(ToolDisplayInformationAttribute.ResourceManagerAssemblyIdentifier)} '{resourceManagerAssemblyIdentifier}'.");
    }

    private GuiToolInstance? GetToolFromInternalName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return AllTools.FirstOrDefault(t => string.Equals(t.InternalComponentName, name, StringComparison.Ordinal));
    }

    private GroupViewItem CreateFavoriteGroupViewItem(IReadOnlyList<GuiToolViewItem> favoriteTools)
    {
        Guard.IsNull(_favoriteToolsGroupViewItem);
        _favoriteToolsGroupViewItem
            = new GroupViewItem(
                iconFontName: "FluentSystemIcons",
                iconGlyph: "\uF70E",
                displayTitle: MainMenu.FavoriteToolsDisplayTitle,
                accessibleName: MainMenu.FavoriteToolsAccessibleName,
                children: new(favoriteTools),
                isExpandedByDefault: true);
        return _favoriteToolsGroupViewItem;
    }

    private static SettingDefinition<bool> CreateIsToolFavoriteSettingDefinition(GuiToolInstance guiToolInstance)
    {
        return new SettingDefinition<bool>($"{guiToolInstance.InternalComponentName}_IsFavorite", defaultValue: false);
    }

    [LoggerMessage(0, LogLevel.Information, "Set '{toolName}' as the most recently used tool.")]
    partial void LogSetMostRecentUsedTool(string toolName);
}
