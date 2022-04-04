#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;

namespace DevToys.Providers.Impl
{
    [Export(typeof(IToolProviderFactory))]
    [Shared]
    internal sealed class ToolProviderFactory : IToolProviderFactory
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly ImmutableArray<ToolProviderViewItem> _allProviders;
        private readonly Task<ImmutableArray<ToolProviderViewItem>> _providersTree;
        private readonly Task<ImmutableArray<ToolProviderViewItem>> _headerProviders;
        private readonly Task<ImmutableArray<ToolProviderViewItem>> _footerProviders;
        private readonly Dictionary<IToolProvider, IToolViewModel> _toolProviderToViewModelCache = new();

        public event EventHandler? IsToolFavoriteChanged;

        [ImportingConstructor]
        public ToolProviderFactory(
            [ImportMany] IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers,
            ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _allProviders = BuildAllTools(providers);
            _providersTree = BuildToolsTreeAsync();
            _headerProviders = BuildHeaderToolsAsync();
            _footerProviders = BuildFooterToolsAsync();

            App.Current.Suspending += OnAppSuspending;
        }

        public IToolViewModel GetToolViewModel(IToolProvider provider)
        {
            if (_toolProviderToViewModelCache.TryGetValue(provider, out IToolViewModel viewModel))
            {
                return viewModel;
            }

            viewModel = provider.CreateTool();
            _toolProviderToViewModelCache[provider] = viewModel;

            return viewModel;
        }

        public async Task<IEnumerable<ToolProviderViewItem>> SearchToolsAsync(string searchQuery)
        {
            await TaskScheduler.Default;

            string[]? searchQueries = searchQuery?.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return
                SortTools(
                    SearchTools(searchQueries).ToList(),
                    takeConsiderationOfMatches: true);
        }

        public async Task<IEnumerable<ToolProviderViewItem>> GetToolsTreeAsync()
        {
            return await _providersTree;
        }

        public IEnumerable<ToolProviderViewItem> GetAllTools()
        {
            return _allProviders;
        }

        public IEnumerable<ToolProviderViewItem> GetAllChildrenTools(IToolProvider toolProvider)
        {
            ToolProviderViewItem? matchedProvider = GetAllTools().FirstOrDefault(item => item.ToolProvider == toolProvider);

            if (matchedProvider is not null)
            {
                var result = GetAllChildrenTools(matchedProvider.ChildrenTools).ToList();
                return SortTools(result, takeConsiderationOfMatches: false);
            }

            return Array.Empty<ToolProviderViewItem>();
        }

        public async Task<IEnumerable<ToolProviderViewItem>> GetHeaderToolsAsync()
        {
            return await _headerProviders;
        }

        public async Task<IEnumerable<ToolProviderViewItem>> GetFooterToolsAsync()
        {
            return await _footerProviders;
        }

        public void SetToolIsFavorite(ToolProviderViewItem toolProviderViewItem, bool isFavorite)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SettingDefinition<bool> isFavoriteSettingDefinition = CreateIsToolFavoriteSettingDefinition(toolProviderViewItem.Metadata);
            _settingsProvider.SetSetting(isFavoriteSettingDefinition, isFavorite);

            foreach (ToolProviderViewItem? tool in GetAllTools())
            {
                if (string.Equals(tool.Metadata.Name, toolProviderViewItem.Metadata.Name, StringComparison.Ordinal))
                {
                    tool.IsFavorite = isFavorite;
                }
            }

            IsToolFavoriteChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task CleanupAsync()
        {
            foreach (IToolViewModel toolViewModel in _toolProviderToViewModelCache.Values)
            {
                try
                {
                    if (toolViewModel is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    if (toolViewModel is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogFault("ToolProviderFactory", ex, $"Unable to cleanup the tool '{toolViewModel.GetType().Name}'.");
                }
            }

            _toolProviderToViewModelCache.Clear();
        }

        private async void OnAppSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            await CleanupAsync();
        }

        private IEnumerable<ToolProviderViewItem> GetAllChildrenTools(IReadOnlyList<ToolProviderViewItem> items)
        {
            foreach (ToolProviderViewItem item in items)
            {
                if (item.ChildrenTools.Count == 0)
                {
                    yield return item;
                }
                else
                {
                    foreach (ToolProviderViewItem child in GetAllChildrenTools(item.ChildrenTools))
                    {
                        yield return child;
                    }
                }
            }
        }

        private IEnumerable<ToolProviderViewItem> SearchTools(string[]? searchQueries)
        {
            if (searchQueries is not null)
            {
                foreach (ToolProviderViewItem provider in GetAllTools())
                {
                    if (!provider.Metadata.NotSearchable                                        // do not search tools marked as non-searchable
                        && provider.ChildrenTools.Count == 0                                    // do not search groups.
                        && !string.IsNullOrWhiteSpace(provider.ToolProvider.SearchDisplayName)) // do not search tools without search display name.
                    {
                        var matches = new List<MatchSpan>();

                        int totalMatchCount = 0;

                        foreach (string? query in searchQueries)
                        {
                            int i = 0;
                            while (i < provider.ToolProvider.SearchDisplayName?.Length && i > -1)
                            {
                                int matchIndex = provider.ToolProvider.SearchDisplayName.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                                if (matchIndex > -1)
                                {
                                    matches.Add(new MatchSpan(matchIndex, query.Length));
                                    i = matchIndex + query.Length;
                                    totalMatchCount++;
                                }

                                i++;
                            }

                            if (provider.ToolProvider.SearchKeywords is not null
                                && !string.IsNullOrEmpty(provider.ToolProvider.SearchKeywords))
                            {
                                string searchKeyword = System.Text.RegularExpressions.Regex.Replace(provider.ToolProvider.SearchKeywords, @"\s", "");
                                i = 0;
                                while (i < searchKeyword.Length && i > -1)
                                {
                                    int matchIndex = searchKeyword.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                                    if (matchIndex > -1)
                                    {
                                        i = matchIndex + query.Length;
                                        totalMatchCount++;
                                    }

                                    i++;
                                }
                            }

                            i = 0;
                            while (i < provider.ToolProvider.MenuDisplayName?.Length && i > -1)
                            {
                                int matchIndex = provider.ToolProvider.MenuDisplayName.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                                if (matchIndex > -1)
                                {
                                    totalMatchCount++;
                                }

                                i++;
                            }

                            i = 0;
                            while (i < provider.ToolProvider.Description?.Length && i > -1)
                            {
                                int matchIndex = provider.ToolProvider.Description.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                                if (matchIndex > -1)
                                {
                                    totalMatchCount++;
                                }

                                i++;
                            }
                        }

                        if (totalMatchCount > 0)
                        {
                            provider.MatchedSpans = matches.ToArray();
                            provider.TotalMatchCount = totalMatchCount;
                            yield return provider;
                        }
                    }
                }
            }
        }

        private IEnumerable<ToolProviderViewItem> SortTools(IReadOnlyList<ToolProviderViewItem> providers, bool takeConsiderationOfMatches)
        {
            foreach (ToolProviderViewItem provider in providers)
            {
                provider.ChildrenTools = SortTools(provider.ChildrenTools, takeConsiderationOfMatches).ToList();
            }

            if (takeConsiderationOfMatches)
            {
                return
                    providers
                        .OrderByDescending(item => item.MatchedSpans.Length)
                        .ThenByDescending(item => item.TotalMatchCount)
                        .ThenBy(item => item.Metadata.Order ?? int.MaxValue)
                        .ThenBy(item => item.ToolProvider.MenuDisplayName)
                        .ThenBy(item => item.Metadata.Name);
            }
            else
            {
                return
                    providers
                        .OrderBy(item => item.Metadata.Order ?? int.MaxValue)
                        .ThenBy(item => item.ToolProvider.MenuDisplayName)
                        .ThenBy(item => item.Metadata.Name);
            }
        }

        private async Task<ImmutableArray<ToolProviderViewItem>> BuildToolsTreeAsync()
        {
            await TaskScheduler.Default;

            var results = new List<ToolProviderViewItem>();
            IEnumerable<ToolProviderViewItem> ToolProviderViewItems
                = GetAllTools()
                    .Where(item => item.Metadata.MenuPlacement == MenuPlacement.Body);

            foreach (ToolProviderViewItem provider in ToolProviderViewItems)
            {
                string parentName = provider.Metadata.Parent;
                if (string.IsNullOrEmpty(parentName))
                {
                    // This tool has no parent, therefore it's a root item. We can just return it.
                    results.Add(provider);
                }
                else
                {
                    // Look for the parent provider
                    ToolProviderViewItem? parentProvider
                        = ToolProviderViewItems.SingleOrDefault(p => string.Equals(parentName, p.Metadata.Name, StringComparison.Ordinal));

                    if (parentProvider is null)
                    {
                        throw new InvalidOperationException($"Parent provider not found for: {provider.Metadata.Name}");
                    }
                    else if (parentProvider == provider)
                    {
                        throw new InvalidOperationException($"Provider '{provider.Metadata.Name}' can't be its own parent.");
                    }

                    // Add the provider to its parent.
                    parentProvider.AddChildTool(provider);
                }
            }

            return SortTools(results, takeConsiderationOfMatches: false).ToImmutableArray();
        }

        private async Task<ImmutableArray<ToolProviderViewItem>> BuildHeaderToolsAsync()
        {
            await TaskScheduler.Default;

            ImmutableArray<ToolProviderViewItem>.Builder result = ImmutableArray.CreateBuilder<ToolProviderViewItem>();
            foreach (
                ToolProviderViewItem provider
                in
                SortTools(
                    GetAllTools()
                        .Where(
                            item => item.Metadata.MenuPlacement == MenuPlacement.Header || item.IsFavorite)
                        .ToList(),
                    takeConsiderationOfMatches: false))
            {
                if (provider.IsFavorite)
                {
                    result.Add(ToolProviderViewItem.CreateToolProviderViewItemWithLongMenuDisplayName(provider));
                }
                else
                {
                    result.Add(provider);
                }
            }

            return result.ToImmutable();
        }

        private async Task<ImmutableArray<ToolProviderViewItem>> BuildFooterToolsAsync()
        {
            await TaskScheduler.Default;

            ImmutableArray<ToolProviderViewItem>.Builder result = ImmutableArray.CreateBuilder<ToolProviderViewItem>();
            foreach (
                ToolProviderViewItem provider
                in
                SortTools(
                    GetAllTools()
                        .Where(
                            item => item.Metadata.MenuPlacement == MenuPlacement.Footer)
                        .ToList(),
                    takeConsiderationOfMatches: false))
            {
                result.Add(provider);
            }

            return result.ToImmutable();
        }

        private ImmutableArray<ToolProviderViewItem> BuildAllTools(IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ToolProviderViewItems
                = providers.Select(
                    item =>
                    {
                        SettingDefinition<bool> isFavoriteSettingDefinition = CreateIsToolFavoriteSettingDefinition(item.Metadata);
                        bool isFavorite = _settingsProvider.GetSetting(isFavoriteSettingDefinition);
                        return new ToolProviderViewItem(item.Metadata, item.Value, isFavorite);
                    })
                .ToList();

            return
                SortTools(ToolProviderViewItems, takeConsiderationOfMatches: false)
                .ToImmutableArray();
        }

        private SettingDefinition<bool> CreateIsToolFavoriteSettingDefinition(ToolProviderMetadata metadata)
        {
            return new SettingDefinition<bool>($"{metadata.Name}_IsFavorite", isRoaming: true, defaultValue: false);
        }
    }
}
