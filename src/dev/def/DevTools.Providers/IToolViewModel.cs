using System.ComponentModel;

namespace DevTools.Providers
{
    /// <summary>
    /// Provides a view model for a tool.
    /// </summary>
    public interface IToolViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets an instance of the view to display in the UI.
        /// </summary>
        IToolView View { get; }
    }
}
