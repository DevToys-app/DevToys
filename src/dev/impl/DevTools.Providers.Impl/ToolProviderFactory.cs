#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace DevTools.Providers.Impl
{
    [Export(typeof(IToolProviderFactory))]
    internal sealed class ToolProviderFactory : IToolProviderFactory
    {
        private readonly IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> _providers;

        [ImportingConstructor]
        public ToolProviderFactory(
            [ImportMany] IEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> providers)
        {
            _providers = providers;
        }

        public IEnumerable<MatchedToolProvider> GetTools(string? searchQuery)
        {
            IOrderedEnumerable<Lazy<IToolProvider, ToolProviderMetadata>> orderedProviders
                = _providers
                    .Where(item => !item.Metadata.IsFooterItem)
                    .OrderBy(item => item.Metadata.Order ?? int.MaxValue)
                    .ThenBy(item => item.Value.DisplayName)
                    .ThenBy(item => item.Metadata.Name);

            searchQuery = searchQuery?.Trim();
            string[]? searchQueries = searchQuery?.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (Lazy<IToolProvider, ToolProviderMetadata> provider in orderedProviders)
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
                            }

                            i++;
                        }
                    }
                }

                if (matches.Count > 0 || searchQueries?.Length == 0)
                {
                    yield return new MatchedToolProvider(provider.Value, matches.ToArray());
                }
            }
        }

        public IEnumerable<MatchedToolProvider> GetFooterTools()
        {
            var matches = Array.Empty<MatchSpan>();
            foreach (Lazy<IToolProvider, ToolProviderMetadata> provider in _providers.Where(item => item.Metadata.IsFooterItem))
            {
                yield return new MatchedToolProvider(provider.Value, matches);
            }
        }
    }
}
