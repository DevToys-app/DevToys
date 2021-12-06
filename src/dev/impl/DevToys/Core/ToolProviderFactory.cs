#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;

namespace DevToys.Providers.Impl
{
    [Export(typeof(IToolProviderFactory))]
    internal sealed class ToolProviderFactory : IToolProviderFactory
    {
        private readonly ImmutableArray<MatchedToolProvider> _allProviders;
        private readonly Task<ImmutableArray<MatchedToolProvider>> _providersTree;
        private readonly Task<ImmutableArray<MatchedToolProvider>> _footerProviders;
        private readonly Dictionary<IToolProvider, IToolViewModel> _toolProviderToViewModelCache = new();

        [ImportingConstructor]
        public ToolProviderFactory(
            [ImportMany] IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers)
        {
            _allProviders = BuildAllTools(providers);
            _providersTree = BuildToolsTreeAsync();
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

        public async Task<IEnumerable<MatchedToolProvider>> SearchToolsAsync(string searchQuery)
        {
            await TaskScheduler.Default;

            string[]? searchQueries = searchQuery?.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return
                SortTools(
                    SearchTools(
                        GetAllTools(),
                        searchQueries)
                    .ToList());
        }

        public async Task<IEnumerable<MatchedToolProvider>> GetToolsTreeAsync()
        {
            return await _providersTree;
        }

        public IEnumerable<MatchedToolProvider> GetAllTools()
        {
            return _allProviders;
        }

        public async Task<IEnumerable<MatchedToolProvider>> GetFooterToolsAsync()
        {
            return await _footerProviders;
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

        private IEnumerable<MatchedToolProvider> SearchTools(IEnumerable<MatchedToolProvider> providers, string[]? searchQueries)
        {
            if (searchQueries is not null)
            {
                foreach (MatchedToolProvider provider in providers)
                {
                    var matches = new List<MatchSpan>();

                    foreach (string? query in searchQueries)
                    {
                        int i = 0;
                        while (i < provider.ToolProvider.DisplayName?.Length && i > -1)
                        {
                            int matchIndex = provider.ToolProvider.DisplayName.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                            if (matchIndex > -1)
                            {
                                matches.Add(new MatchSpan(matchIndex, query.Length));
                                i = matchIndex + query.Length;
                            }

                            i++;
                        }
                    }

                    if (matches.Count > 0)
                    {
                        // Return a new MatchedToolProvider with the matches.
                        yield return
                            new MatchedToolProvider(
                                provider.Metadata,
                                provider.ToolProvider,
                                matches.ToArray());
                    }
                }
            }
        }

        private IEnumerable<MatchedToolProvider> SortTools(IReadOnlyList<MatchedToolProvider> providers)
        {
            foreach (MatchedToolProvider provider in providers)
            {
                provider.ChildrenTools = SortTools(provider.ChildrenTools).ToList();
            }

            return
                providers
                    .OrderByDescending(item => item.MatchedSpans.Length)
                    .ThenBy(item => item.Metadata.Order ?? int.MaxValue)
                    .ThenBy(item => item.ToolProvider.DisplayName)
                    .ThenBy(item => item.Metadata.Name);
        }

        private async Task<ImmutableArray<MatchedToolProvider>> BuildToolsTreeAsync()
        {
            await TaskScheduler.Default;

            var results = new List<MatchedToolProvider>();
            IEnumerable<MatchedToolProvider> matchedToolProviders
                = GetAllTools()
                    .Where(item => !item.Metadata.IsFooterItem);

            foreach (MatchedToolProvider provider in matchedToolProviders)
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
                    MatchedToolProvider? parentProvider
                        = matchedToolProviders.SingleOrDefault(p => string.Equals(parentName, p.Metadata.Name, StringComparison.Ordinal));

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

            return SortTools(results).ToImmutableArray();
        }

        private async Task<ImmutableArray<MatchedToolProvider>> BuildFooterToolsAsync()
        {
            await TaskScheduler.Default;

            ImmutableArray<MatchedToolProvider>.Builder result = ImmutableArray.CreateBuilder<MatchedToolProvider>();
            foreach (MatchedToolProvider provider in SortTools(GetAllTools().Where(item => item.Metadata.IsFooterItem).ToList()))
            {
                result.Add(provider);
            }

            return result.ToImmutable();
        }

        private ImmutableArray<MatchedToolProvider> BuildAllTools(IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var matchedToolProviders
                = providers.Select(
                    item => new MatchedToolProvider(item.Metadata, item.Value))
                .ToList();

            return
                SortTools(matchedToolProviders)
                .ToImmutableArray();
        }
    }
}
