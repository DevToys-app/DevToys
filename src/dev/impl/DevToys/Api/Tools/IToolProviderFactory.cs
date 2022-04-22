#nullable enable

using System;
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
        Task<IEnumerable<ToolProviderViewItem>> SearchToolsAsync(string searchQuery);

        /// <summary>
        /// Gets a hierarchical list of available tools. This does not include footer tools.
        /// </summary>
        Task<IEnumerable<ToolProviderViewItem>> GetToolsTreeAsync();

        /// <summary>
        /// Gets a flat list containing all the tools available.
        /// </summary>
        IEnumerable<ToolProviderViewItem> GetAllTools();

        /// <summary>
        /// Gets a flat list of all the children and sub-children of a given tool provider.
        /// </summary>
        IEnumerable<ToolProviderViewItem> GetAllChildrenTools(IToolProvider toolProvider);

        /// <summary>
        /// Gets the list of tools available that have should be displayed in the header.
        /// </summary>
        Task<IEnumerable<ToolProviderViewItem>> GetHeaderToolsAsync();

        /// <summary>
        /// Gets the list of tools available that have should be displayed in the footer.
        /// </summary>
        Task<IEnumerable<ToolProviderViewItem>> GetFooterToolsAsync();

        /// <summary>
        /// Sets whether the given tool is favorite or not.
        /// </summary>
        void SetToolIsFavorite(ToolProviderViewItem toolProviderViewItem, bool isFavorite);

        /// <summary>
        /// Called when the app is shutting down. Asks every tools to cleanup resources.
        /// </summary>
        Task CleanupAsync();
    }
}
