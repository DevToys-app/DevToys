using System;
using System.ComponentModel;

namespace DevTools.Providers
{
    /// <summary>
    /// Provides a view model for a tool.
    /// </summary>
    public interface IToolViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Get the type of the view to display in the UI.
        /// </summary>
        /// <remarks>The type must be a page</remarks>
        Type View { get; }
    }
}
