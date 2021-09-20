using DevTools.Core.Injection;

namespace DevTools.Core.Navigation
{
    /// <summary>
    /// Represents the arguments passed to a page or frame.
    /// </summary>
    public class NavigationParameter
    {
        public IMefProvider ExportProvider { get; }

        public string? Query { get; }

        public object? ViewModel { get; }

        public string? ClipBoardContent { get; }

        public NavigationParameter(IMefProvider exportProvider, object? viewModel, string? clipBoardContent = null, string? query = null)
        {
            ExportProvider = Arguments.NotNull(exportProvider, nameof(exportProvider));
            ViewModel = viewModel;
            ClipBoardContent = clipBoardContent;
            Query = query;
        }
    }
}
