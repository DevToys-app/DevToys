#nullable enable

using System.Collections.Generic;
using System.Linq;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools
{
    internal sealed class SearchResultToolProvider : IToolProvider
    {
        internal static MatchedToolProvider CreateResult(
            string searchQuery,
            IEnumerable<MatchedToolProvider> searchResults)
        {
            return new MatchedToolProvider(
                new ToolProviderMetadata
                {
                    Name = "SearchResult",
                    NoCompactOverlaySupport = true,
                },
                new SearchResultToolProvider(
                    searchQuery,
                    searchResults));
        }

        private readonly string _searchQuery;
        private readonly IEnumerable<MatchedToolProvider> _searchResults;

        private SearchResultToolProvider(
            string searchQuery,
            IEnumerable<MatchedToolProvider> searchResults)
        {
            _searchQuery = searchQuery;
            _searchResults = searchResults;
        }

        public string MenuDisplayName => GetTitle();

        public string? SearchDisplayName => GetTitle();

        public string AccessibleName => GetTitle();

        public TaskCompletionNotifier<IconElement> IconSource => null!;

        public string? Description => null;

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return new GroupToolViewModel(_searchResults.Select(item => item.ToolProvider));
        }

        private string GetTitle()
        {
            if (_searchResults.Any())
            {
                return LanguageManager.Instance.SearchResult.GetFormattedSearchResults(_searchQuery);
            }

            return LanguageManager.Instance.SearchResult.NoResultsFound;
        }
    }
}
