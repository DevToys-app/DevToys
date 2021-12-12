#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Factory allowing to get a set of <see cref="IToolProvider"/>.
    /// </summary>
    public interface IToolProviderFactory
    {
        /// <summary>
        /// Get a tool view model
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        IToolViewModel GetToolViewModel(IToolProvider provider);

        /// <summary>
        /// Gets a flat list of tools that match the given query.
        /// </summary>
        Task<IEnumerable<MatchedToolProvider>> SearchToolsAsync(string searchQuery);

        /// <summary>
        /// Gets a hierarchical list of available tools. This does not include footer tools.
        /// </summary>
        Task<IEnumerable<MatchedToolProvider>> GetToolsTreeAsync();

        /// <summary>
        /// Gets a flat list containing all the tools available.
        /// </summary>
        IEnumerable<MatchedToolProvider> GetAllTools();

        /// <summary>
        /// Gets a flat list of all the children and sub-children of a given tool provider.
        /// </summary>
        IEnumerable<IToolProvider> GetAllChildrenTools(IToolProvider toolProvider);

        /// <summary>
        /// Gets the list of tools available that have should be displayed in the header.
        /// </summary>
        Task<IEnumerable<MatchedToolProvider>> GetHeaderToolsAsync();

        /// <summary>
        /// Gets the list of tools available that have should be displayed in the footer.
        /// </summary>
        Task<IEnumerable<MatchedToolProvider>> GetFooterToolsAsync();

        /// <summary>
        /// Called when the app is shutting down. Asks every tools to cleanup resources.
        /// </summary>
        Task CleanupAsync();
    }
}
