using DevTools.Core.Injection;
using System.ComponentModel;

namespace DevTools.Core.Navigation
{
    /// <summary>
    /// Represents the arguments passed to a page or frame.
    /// </summary>
    public class NavigationParameter
    {
        /// <summary>
        /// Gets the MEF exporter.
        /// </summary>
        public IMefProvider ExportProvider { get; }

        /// <summary>
        /// Gets the query used when opening the app through the URI protocol.
        /// </summary>
        public string? Query { get; }

        /// <summary>
        /// Gets the view model to apply to the page we navigate to.
        /// </summary>
        public INotifyPropertyChanged? ViewModel { get; }

        /// <summary>
        /// Gets the text that is in the clipboard. This value is used for the Smart Detection feature.
        /// </summary>
        public string? ClipBoardContent { get; }

        public NavigationParameter(
            IMefProvider exportProvider,
            INotifyPropertyChanged? viewModel = null,
            string? clipBoardContent = null,
            string? query = null)
        {
            ExportProvider = Arguments.NotNull(exportProvider, nameof(exportProvider));
            ViewModel = viewModel;
            ClipBoardContent = clipBoardContent;
            Query = query;
        }
    }
}
