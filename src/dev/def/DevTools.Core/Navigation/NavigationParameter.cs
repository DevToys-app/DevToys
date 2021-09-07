using DevTools.Core.Injection;

namespace DevTools.Core.Navigation
{
    /// <summary>
    /// Represents the arguments passed to a page or frame.
    /// </summary>
    public class NavigationParameter
    {
        public IMefProvider ExportProvider { get; }

        public object? Parameter { get; }

        public NavigationParameter(IMefProvider exportProvider, object? parameter)
        {
            ExportProvider = Arguments.NotNull(exportProvider, nameof(exportProvider));
            Parameter = parameter;
        }
    }
}
