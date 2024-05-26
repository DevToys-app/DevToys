using System.Collections.ObjectModel;
using System.Reflection;
using DevToys.Core.Tools.Metadata;
using DevToys.Core.Tools.ViewItems;
using DevToys.Localization;
using DevToys.Localization.Strings.MainWindow;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Microsoft.Extensions.Logging;
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
                        iconFontName: "FluentSystemIcons",
                        iconGlyph: "\uF3E9",
                        groupName: "N/A",
                        shortDisplayTitleResourceName: "N/A",
                        longDisplayTitleResourceName: nameof(MainWindow.SearchNoResultsFound),
                        resourceManagerAssemblyIdentifier: nameof(DevToysLocalizationResourceManagerAssemblyIdentifier),
                        resourceManagerBaseName: "DevToys.Localization.Strings.MainWindow.MainWindow")),
                typeof(DevToysLocalizationResourceManagerAssemblyIdentifier).Assembly),
            showLongDisplayTitle: true);

    private static readonly char[] WordSeparator = new char[] { ' ', '\t' };

    private readonly ILogger _logger;
    private readonly IEnumerable<Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata>> _resourceAssemblyIdentifiers;
    private readonly IEnumerable<Lazy<GuiToolGroup, GuiToolGroupMetadata>> _guiToolGroups;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IReadOnlyList<GuiToolInstance> _footerToolInstances;
    private readonly IReadOnlyList<GuiToolInstance> _bodyToolInstances;

    private ObservableCollection<INotifyPropertyChanged?>? _headerAndBodyToolViewItems;
    private ReadOnlyObservableCollection<INotifyPropertyChanged?>? _headerAndBodyToolViewItemsReadOnly;
    private ObservableCollection<INotifyPropertyChanged?>? _footerToolViewItems;
    private ReadOnlyObservableCollection<INotifyPropertyChanged?>? _footerToolViewItemsReadOnly;
    private GroupViewItem? _favoriteToolsGroupViewItem;
    private SeparatorViewItem? _separatorAfterAllToolsItem;
    private SeparatorViewItem? _separatorAfterRecentTools;

    [ImportingConstructor]
    public GuiToolProvider(
        [ImportMany] IEnumerable<Lazy<IGuiTool, GuiToolMetadata>> guiTools,
        [ImportMany] IEnumerable<Lazy<GuiToolGroup, GuiToolGroupMetadata>> guiToolGroups,
        [ImportMany] IEnumerable<Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata>> resourceAssemblyIdentifiers,
        ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _resourceAssemblyIdentifiers = resourceAssemblyIdentifiers;
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
    /// This includes "All tools" menu item, recent and favorites.
    /// </summary>
    public ReadOnlyObservableCollection<INotifyPropertyChanged?> HeaderAndBodyToolViewItems
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
    public ReadOnlyObservableCollection<INotifyPropertyChanged?> FooterToolViewItems
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
    /// Raised when the list of favorite tools changed.
    /// </summary>
    public event EventHandler? FavoriteToolsChanged;

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
            Guard.IsNotNull(_headerAndBodyToolViewItems[1]);
            Guard.IsOfType<SeparatorViewItem>(_headerAndBodyToolViewItems[1]!);

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

        FavoriteToolsChanged?.Invoke(this, EventArgs.Empty);
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
            INotifyPropertyChanged? item = FooterToolViewItems[i];
            if (item is GuiToolViewItem guiToolViewItem && guiToolViewItem.ToolInstance == guiToolInstance)
            {
                yield return guiToolViewItem;
            }
        }

        for (int i = 0; i < HeaderAndBodyToolViewItems.Count; i++)
        {
            INotifyPropertyChanged? item = HeaderAndBodyToolViewItems[i];
            if (item is GuiToolViewItem guiToolViewItem && guiToolViewItem.ToolInstance == guiToolInstance)
            {
                yield return guiToolViewItem;
            }
            else if (item is GroupViewItem groupViewItem && groupViewItem.Children is not null)
            {
                for (int j = 0; j < groupViewItem.Children.Count; j++)
                {
                    GuiToolViewItem subGuiToolViewItem = groupViewItem.Children[j];
                    if (subGuiToolViewItem.ToolInstance == guiToolInstance)
                    {
                        yield return subGuiToolViewItem;
                    }
                }
            }
        }
    }

    public void ForEachToolViewItem(Action<GuiToolViewItem> action)
    {
        for (int i = 0; i < FooterToolViewItems.Count; i++)
        {
            INotifyPropertyChanged? item = FooterToolViewItems[i];
            if (item is GuiToolViewItem guiToolViewItem)
            {
                action(guiToolViewItem);
            }
        }

        for (int i = 0; i < HeaderAndBodyToolViewItems.Count; i++)
        {
            INotifyPropertyChanged? item = HeaderAndBodyToolViewItems[i];
            if (item is GuiToolViewItem guiToolViewItem)
            {
                action(guiToolViewItem);
            }
            else if (item is GroupViewItem groupViewItem && groupViewItem.Children is not null)
            {
                for (int j = 0; j < groupViewItem.Children.Count; j++)
                {
                    GuiToolViewItem subGuiToolViewItem = groupViewItem.Children[j];
                    action(subGuiToolViewItem);
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

        string[] searchQueries = searchQuery.Split(WordSeparator, StringSplitOptions.RemoveEmptyEntries);

        var weightedToolList = new List<(double weight, GuiToolViewItem tool)>();
        for (int i = 0; i < AllTools.Count; i++)
        {
            GuiToolInstance tool = AllTools[i];

            if (!tool.NotSearchable) // do not search tools marked as non-searchable
            {
                SearchTool(searchQueries, tool, out TextSpan[] matchedSpans, out double weight);

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
            var descOrderedWeightedToolList = weightedToolList.OrderByDescending(i => i.weight).ToList(); // Order by weight.
            int thirdQuarterItemIndex = Math.Min((int)(0.25 * descOrderedWeightedToolList.Count), descOrderedWeightedToolList.Count - 1); // Get the 3/4 item index in the list.
            double thirdQuarterWeight = descOrderedWeightedToolList[thirdQuarterItemIndex].weight; // Get the 3/4 item weight.

            searchResultListToUpdate
                .AddRange(
                    descOrderedWeightedToolList
                        .Take(5) // Take the 5 first items.
                        .TakeWhile(i => i.weight >= thirdQuarterWeight) // Take items with a weight greater or equal to the 3/4 item weight. This is to avoid showing too many items with a low weight.
                        .Select(i => i.tool));
        }

        if (searchResultListToUpdate.Count == 0)
        {
            searchResultListToUpdate.Add(NoResultFoundItem);
        }
    }

    public GuiToolInstance? GetToolFromInternalName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return AllTools.FirstOrDefault(t => string.Equals(t.InternalComponentName, name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Dispose every disposable <see cref="IGuiTool"/>.
    /// </summary>
    public void DisposeTools()
    {
        for (int i = 0; i < AllTools.Count; i++)
        {
            AllTools[i].Dispose();
        }
    }

    private void BuildGuiToolInstances(
        IEnumerable<Lazy<IGuiTool, GuiToolMetadata>> guiTools,
        out IReadOnlyList<GuiToolInstance> allTools,
        out IReadOnlyList<GuiToolInstance> footerTools,
        out IReadOnlyList<GuiToolInstance> bodyTools)
    {
        DateTime startTime = DateTime.UtcNow;

        var allToolInstances = new List<GuiToolInstance>();
        var footerToolInstances = new List<GuiToolInstance>();
        var bodyToolInstances = new List<GuiToolInstance>();

        // Order all the tools.
        try
        {
            guiTools
                = ExtensionOrderer.Order(
                    guiTools
                        .OrderBy(tool => tool.Metadata.MenuPlacement)
                        .ThenBy(tool => tool.Metadata.InternalComponentName));
        }
        catch (Exception ex)
        {
            // TODO: We should let the user know that something went wrong.
            LogOrderingToolsFailed(ex);
        }

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
                resourceManagerAssembly = GetResourceManagerAssembly(guiToolDefinition.Metadata);
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

        double elapsedMilliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;
        LogToolInstancesCreated(allToolInstances.Count, elapsedMilliseconds);
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
            iconGlyph: '\uE70F',
            displayTitle: MainWindow.AllToolsDisplayTitle,
            accessibleName: MainWindow.AllToolsAccessibleName);

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
                LogGroupNotFound(tool.GroupName, tool.InternalComponentName);
                continue;
            }

            // Create a group view presentation, if needed.
            if (!groups.TryGetValue(tool.GroupName, out Lazy<GroupViewItem, GuiToolGroupMetadata>? groupViewItem))
            {
                groupViewItem = new(() => new GroupViewItem(toolGroup.Metadata.InternalComponentName, toolGroup.Value), toolGroup.Metadata);
                groups[tool.GroupName] = groupViewItem;
            }

            // Add the tool to the group.
            Guard.IsNotNull(groupViewItem.Value.Children);
            groupViewItem.Value.Children.Add(new GuiToolViewItem(tool, showLongDisplayTitle: false));
        }

        // Order tools groups.
        try
        {
            IEnumerable<GroupViewItem> orderedGroups
                = ExtensionOrderer.Order(
                    groups.Values.OrderBy(g => g.Value.DisplayTitle))
                .Select(g => g.Value);
            return orderedGroups;
        }
        catch (Exception ex)
        {
            // TODO: We should let the user know that something went wrong.
            LogOrderingGroupsFailed(ex);
        }

        // Fallback
        return groups.Values.Select(g => g.Value);
    }

    private Assembly? GetResourceManagerAssembly(GuiToolMetadata guiToolMetadata)
    {
        foreach (Lazy<IResourceAssemblyIdentifier, ResourceAssemblyIdentifierMetadata> item in _resourceAssemblyIdentifiers)
        {
            if (string.Equals(item.Metadata.InternalComponentName, guiToolMetadata.ResourceManagerAssemblyIdentifier, StringComparison.Ordinal))
            {
                return item.Value.GetType().Assembly;
            }
        }

        LogResourceManagerAssemblyIdentifierFound(guiToolMetadata.InternalComponentName, guiToolMetadata.ResourceManagerAssemblyIdentifier);
        return null;
    }

    private GroupViewItem CreateFavoriteGroupViewItem(IReadOnlyList<GuiToolViewItem> favoriteTools)
    {
        Guard.IsNull(_favoriteToolsGroupViewItem);
        _favoriteToolsGroupViewItem
            = new GroupViewItem(
                ReservedGuiToolGroupNames.FavoriteTools,
                iconFontName: "FluentSystemIcons",
                iconGlyph: '\uE70F',
                displayTitle: MainWindow.FavoriteToolsDisplayTitle,
                accessibleName: MainWindow.FavoriteToolsAccessibleName,
                children: new(favoriteTools),
                menuItemShouldBeExpandedByDefault: true);
        return _favoriteToolsGroupViewItem;
    }

    private static SettingDefinition<bool> CreateIsToolFavoriteSettingDefinition(GuiToolInstance guiToolInstance)
    {
        return new SettingDefinition<bool>($"{guiToolInstance.InternalComponentName}_IsFavorite", defaultValue: false);
    }

    private static void SearchTool(string[] searchQueries, GuiToolInstance tool, out TextSpan[] matchedSpans, out double weight)
    {
        var matches = new List<TextSpan>();
        weight = 0;
        foreach (string? query in searchQueries)
        {
            WeightMatch(query, tool.LongOrShortDisplayTitle, out double titleWeight, out IReadOnlyList<TextSpan> spans);
            WeightMatch(query, tool.SearchKeywords, out double searchKeywordsWeight, out _);
            WeightMatch(query, tool.Description, out double descriptionWeight, out _);

            searchKeywordsWeight *= 0.75; // reduce the importance of this weight by 25%.
            descriptionWeight *= 0.15; // reduce the importance of this weight by 85%.

            weight = weight + titleWeight + searchKeywordsWeight + descriptionWeight;
            matches.AddRange(spans);
        }

        // Sort spans and make sure no span overlap.
        matchedSpans = CleanUpMatchedSpans(matches);
    }

    private static void WeightMatch(
        string searchQuery,
        string stringToTestAgainst,
        out double weight,
        out IReadOnlyList<TextSpan> matchSpans)
    {
        var matches = new List<TextSpan>();
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
                matches.Add(new TextSpan(matchIndex, searchQuery.Length));
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

        string[] stringToTestAgainstSplitted = stringToTestAgainst.Split(WordSeparator, StringSplitOptions.RemoveEmptyEntries);
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

    private static TextSpan[] CleanUpMatchedSpans(List<TextSpan>? spans)
    {
        if (spans is null)
        {
            return Array.Empty<TextSpan>();
        }

        spans.Sort((x, y) => x.StartPosition.CompareTo(y.StartPosition));

        for (int i = 0; i < spans.Count - 1; i++)
        {
            if (spans[i].EndPosition > spans[i + 1].StartPosition)
            {
                if (spans[i].EndPosition >= spans[i + 1].EndPosition)
                {
                    spans.RemoveAt(i + 1);
                    i--;
                }
                else
                {
                    spans[i]
                        = new TextSpan(
                            spans[i].StartPosition,
                            spans[i].Length - (spans[i].EndPosition - spans[i + 1].StartPosition));
                }
            }
        }

        return spans.ToArray();
    }

    [LoggerMessage(0, LogLevel.Information, "Set '{toolName}' as the most recently used tool.")]
    partial void LogSetMostRecentUsedTool(string toolName);

    [LoggerMessage(1, LogLevel.Error, "Error while ordering tools.")]
    partial void LogOrderingToolsFailed(Exception ex);

    [LoggerMessage(2, LogLevel.Error, "Error while ordering groups.")]
    partial void LogOrderingGroupsFailed(Exception ex);

    [LoggerMessage(3, LogLevel.Information, "Instantiated {toolCount} tools in {duration}ms")]
    partial void LogToolInstancesCreated(int toolCount, double duration);

    [LoggerMessage(4, LogLevel.Error, "Unable to find the group named '{groupName}' that the tool '{toolName}' is pointing to.")]
    partial void LogGroupNotFound(string groupName, string toolName);

    [LoggerMessage(5, LogLevel.Error, "Unable to find the ResourceManagerAssemblyIdentifier '{resourceManagerAssemblyIdentifier}' that the tool '{toolName}' is pointing to.")]
    partial void LogResourceManagerAssemblyIdentifierFound(string toolName, string resourceManagerAssemblyIdentifier);
}
