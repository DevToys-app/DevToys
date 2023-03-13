using System.Collections.ObjectModel;
using System.Reflection;
using DevToys.Api;
using DevToys.Api.Core;
using DevToys.Core.Tools.Metadata;
using DevToys.Core.Tools.ViewItems;
using DevToys.Localization;
using DevToys.Localization.Strings.MainMenu;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;

namespace DevToys.Core.Tools;

/// <summary>
/// Provides information about tools in the app.
/// </summary>
[Export(typeof(GuiToolProvider))]
public sealed partial class GuiToolProvider
{
    public static readonly GuiToolViewItem NoResultFoundItem
        = new(
            new GuiToolInstance(
                new Lazy<IGuiTool, GuiToolMetadata>(
                    GuiToolMetadata.Create(
                        internalComponentName: "NoSearchResults",
                        author: "N/A",
                        iconFontName: "FluentSystemIcons",
                        iconGlyph: "\uF3E9",
                        groupName: "N/A",
                        shortDisplayTitleResourceName: "N/A",
                        longDisplayTitleResourceName: nameof(MainMenu.SearchNoResultsFound),
                        resourceManagerAssemblyIdentifier: nameof(DevToysLocalizationResourceManagerAssemblyIdentifier),
                        resourceManagerBaseName: "DevToys.Localization.Strings.MainMenu.MainMenu")),
                typeof(DevToysLocalizationResourceManagerAssemblyIdentifier).Assembly),
            showLongDisplayTitle: true);

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

        string recentTool1 = _settingsProvider.GetSetting(PredefinedSettings.RecentTool1);
        string recentTool2 = _settingsProvider.GetSetting(PredefinedSettings.RecentTool2);

        if (string.Equals(guiToolInstance.InternalComponentName, recentTool1, StringComparison.Ordinal))
        {
            // The tool is already the most recently used one.
        }
        else if (string.Equals(guiToolInstance.InternalComponentName, recentTool2, StringComparison.Ordinal))
        {
            // Move second most recent to the most recent.
            _settingsProvider.SetSetting(PredefinedSettings.RecentTool2, recentTool1);
            _settingsProvider.SetSetting(PredefinedSettings.RecentTool1, guiToolInstance.InternalComponentName);
        }
        else
        {
            // Shift first and second most recent to the second and third items. Add the given item as the most recent one.
            _settingsProvider.SetSetting(PredefinedSettings.RecentTool3, recentTool2);
            _settingsProvider.SetSetting(PredefinedSettings.RecentTool2, recentTool1);
            _settingsProvider.SetSetting(PredefinedSettings.RecentTool1, guiToolInstance.InternalComponentName);
        }

        LogSetMostRecentUsedTool(guiToolInstance.InternalComponentName);
    }

    /// <summary>
    /// Gets the list of most recently used tools.
    /// </summary>
    public IEnumerable<GuiToolInstance> GetMostRecentUsedTools()
    {
        GuiToolInstance? recentTool1 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool1));
        GuiToolInstance? recentTool2 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool2));
        GuiToolInstance? recentTool3 = GetToolFromInternalName(_settingsProvider.GetSetting(PredefinedSettings.RecentTool3));

        if (recentTool1 is not null)
        {
            yield return recentTool1;
        }

        if (recentTool2 is not null)
        {
            yield return recentTool2;
        }

        if (recentTool3 is not null)
        {
            yield return recentTool3;
        }
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

    /// <summary>
    /// Search the given <paramref name="searchQuery"/> in the list of known tools and update the given <paramref name="searchResultListToUpdate"/>
    /// to reflect the result.
    /// </summary>
    public void SearchTools(string searchQuery, ObservableCollection<GuiToolViewItem> searchResultListToUpdate)
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            searchResultListToUpdate.Clear();
            return;
        }

        string[] searchQueries = searchQuery.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        var weightedToolList = new List<(double weight, GuiToolViewItem tool)>();
        for (int i = 0; i < AllTools.Count; i++)
        {
            GuiToolInstance tool = AllTools[i];

            if (!tool.NotSearchable                                     // do not search tools marked as non-searchable
                && !string.IsNullOrWhiteSpace(tool.LongDisplayTitle))   // do not search tools without long display name.
            {
                SearchTool(searchQueries, tool, out MatchSpan[] matchedSpans, out double weight);

                if (weight > 0)
                {
                    weightedToolList.Add(
                        new(
                            weight,
                            new GuiToolViewItem(
                                tool,
                                showLongDisplayTitle: true,
                                matchedSpans)));
                }
            }
        }

        searchResultListToUpdate.Clear();

        if (weightedToolList.Count > 0)
        {
            var descOrderedWeightedToolList = weightedToolList.OrderByDescending(i => i.weight).ToList();
            int thirdQuarterItemIndex = Math.Min((int)(0.25 * descOrderedWeightedToolList.Count), descOrderedWeightedToolList.Count - 1);
            double thirdQuarterWeight = descOrderedWeightedToolList[thirdQuarterItemIndex].weight;

            searchResultListToUpdate
                .AddRange(
                    descOrderedWeightedToolList
                        .Take(5)
                        .TakeWhile(i => i.weight >= thirdQuarterWeight)
                        .Select(i => i.tool));
        }

        if (searchResultListToUpdate.Count == 0)
        {
            searchResultListToUpdate.Add(NoResultFoundItem);
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
        guiTools = ExtensionOrderer.Order(guiTools.OrderBy(tool => tool.Metadata.MenuPlacement));

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
            ReservedGuiToolGroupNames.AllTools,
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
        bool anyRecentTool = false;

        foreach (GuiToolInstance recentTool in GetMostRecentUsedTools())
        {
            anyRecentTool = true;
            yield return new GuiToolViewItem(recentTool);
        }

        if (anyRecentTool)
        {
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
                groupViewItem = new(() => new GroupViewItem(toolGroup.Metadata.InternalComponentName, toolGroup.Value), toolGroup.Metadata);
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
                ReservedGuiToolGroupNames.FavoriteTools,
                iconFontName: "FluentSystemIcons",
                iconGlyph: "\uF70E",
                displayTitle: MainMenu.FavoriteToolsDisplayTitle,
                accessibleName: MainMenu.FavoriteToolsAccessibleName,
                children: new(favoriteTools),
                menuItemShouldBeExpandedByDefault: true);
        return _favoriteToolsGroupViewItem;
    }

    private static SettingDefinition<bool> CreateIsToolFavoriteSettingDefinition(GuiToolInstance guiToolInstance)
    {
        return new SettingDefinition<bool>($"{guiToolInstance.InternalComponentName}_IsFavorite", defaultValue: false);
    }

    private static void SearchTool(string[] searchQueries, GuiToolInstance tool, out MatchSpan[] matchedSpans, out double weight)
    {
        var matches = new List<MatchSpan>();
        weight = 0;
        foreach (string? query in searchQueries)
        {
            WeightMatch(query, tool.LongDisplayTitle, out double titleWeight, out IReadOnlyList<MatchSpan> spans);
            WeightMatch(query, tool.SearchKeywords, out double searchKeywordsWeight, out _);
            WeightMatch(query, tool.Description, out double descriptionWeight, out _);

            searchKeywordsWeight *= 0.75; // reduce the importance of this weight by 25%.
            descriptionWeight *= 0.15; // reduce the importance of this weight by 85%.

            weight = weight + titleWeight + searchKeywordsWeight + descriptionWeight;
            matches.AddRange(spans);
        }

        matchedSpans = matches.Distinct().ToArray();
    }

    private static void WeightMatch(
        string searchQuery,
        string stringToTestAgainst,
        out double weight,
        out IReadOnlyList<MatchSpan> matchSpans)
    {
        var matches = new List<MatchSpan>();
        matchSpans = matches;
        weight = 0;

        if (string.IsNullOrWhiteSpace(stringToTestAgainst))
        {
            return;
        }

        int i = 0;
        while (i < stringToTestAgainst.Length && i > -1)
        {
            int matchIndex = stringToTestAgainst.IndexOf(searchQuery, i, StringComparison.OrdinalIgnoreCase);
            if (matchIndex > -1)
            {
                matches.Add(new MatchSpan(matchIndex, searchQuery.Length));
                i = matchIndex + searchQuery.Length;

                if (matchIndex > 0 && char.IsLetterOrDigit(stringToTestAgainst[matchIndex - 1]))
                {
                    // Substring match
                    weight += 15;
                }
                else if ((i < stringToTestAgainst.Length && char.IsWhiteSpace(stringToTestAgainst[i])) || i == stringToTestAgainst.Length)
                {
                    // Exact match
                    weight += 100;
                }
                else
                {
                    // Prefix match
                    weight += 35;
                }

                if (matchIndex == 0)
                {
                    // It's the first word of the string! Bonus!
                    weight += 10;
                }
            }

            i++;
        }

        string[] stringToTestAgainstSplitted = stringToTestAgainst.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        for (i = 0; i < stringToTestAgainstSplitted.Length; i++)
        {
            // Fuzzy match
            int fuzzyWeight = Fuzz.WeightedRatio(searchQuery, stringToTestAgainstSplitted[i], PreprocessMode.Full);
            if (fuzzyWeight >= 75)
            {
                weight += 5;
            }
        }
    }

    [LoggerMessage(0, LogLevel.Information, "Set '{toolName}' as the most recently used tool.")]
    partial void LogSetMostRecentUsedTool(string toolName);
}
