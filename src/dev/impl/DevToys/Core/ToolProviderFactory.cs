#nullable enable

using DevToys.Api.Tools;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace DevToys.Providers.Impl
{
    [Export(typeof(IToolProviderFactory))]
    internal sealed class ToolProviderFactory : IToolProviderFactory
    {
        private readonly IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> _providers;
        private readonly Dictionary<IToolProvider, IToolViewModel> _toolProviderToViewModelCache = new();

        [ImportingConstructor]
        public ToolProviderFactory(
            [ImportMany] IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers)
        {
            _providers = providers;
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

        public IEnumerable<MatchedToolProvider> GetTools(string? searchQuery)
        {
            string[]? searchQueries = searchQuery?.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var results = new List<MatchedToolProvider>();
            foreach (Lazy<IToolProvider, ToolProviderMetadata> provider in _providers.Where(item => !item.Metadata.IsFooterItem))
            {
                var matches = new List<MatchSpan>();

                if (searchQueries is not null)
                {
                    foreach (string query in searchQueries)
                    {
                        int i = 0;
                        while (i < provider.Value.DisplayName?.Length && i > -1)
                        {
                            int matchIndex = provider.Value.DisplayName.IndexOf(query, i, StringComparison.OrdinalIgnoreCase);
                            if (matchIndex > -1)
                            {
                                matches.Add(new MatchSpan(matchIndex, query.Length));
                                i = matchIndex + query.Length;
                            }

                            i++;
                        }
                    }
                }

                if (matches.Count > 0 || searchQueries is null || searchQueries.Length == 0)
                {
                    results.Add(
                        new MatchedToolProvider(
                            provider.Metadata,
                            provider.Value,
                            matches.ToArray()));
                }
            }

            return
                results
                    .OrderByDescending(item => item.MatchedSpans.Length)
                    .ThenBy(item => item.Metadata.Order ?? int.MaxValue)
                    .ThenBy(item => item.ToolProvider.DisplayName)
                    .ThenBy(item => item.Metadata.Name);
        }

        public IEnumerable<MatchedToolProvider> GetFooterTools()
        {
            var matches = Array.Empty<MatchSpan>();
            foreach (Lazy<IToolProvider, ToolProviderMetadata> provider
                in _providers.Where(item => item.Metadata.IsFooterItem)
                    .OrderBy(item => item.Metadata.Order ?? int.MaxValue)
                    .ThenBy(item => item.Value.DisplayName)
                    .ThenBy(item => item.Metadata.Name))
            {
                yield return new MatchedToolProvider(provider.Metadata, provider.Value, matches);
            }
        }
    }
}
