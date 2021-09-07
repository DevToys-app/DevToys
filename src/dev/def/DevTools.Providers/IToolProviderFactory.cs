using System.Collections.Generic;

namespace DevTools.Providers
{
    /// <summary>
    /// Factory allowing to get a set of <see cref="IToolProvider"/>.
    /// </summary>
    public interface IToolProviderFactory
    {
        /// <summary>
        /// Gets the list of tools available.
        /// </summary>
        /// <param name="searchQuery">If not null or empty, the method will return items that match the given search query and order them.</param>
        /// <returns>
        /// Returns a list of provider along with an array of parts that matched <paramref name="searchQuery"/>.
        /// </returns>
        IEnumerable<MatchedToolProvider> GetTools(string? searchQuery);

        /// <summary>
        /// Gets the list of tools available that have the <see cref="IsFooterItemAttribute"/>.
        /// </summary>
        IEnumerable<MatchedToolProvider> GetFooterTools();
    }
}
