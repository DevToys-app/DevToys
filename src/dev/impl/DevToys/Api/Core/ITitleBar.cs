#nullable enable

using System.ComponentModel;
using System.Threading.Tasks;

namespace DevToys.Api.Core
{
    /// <summary>
    /// Provides a service designed to manager the window title bar.
    /// </summary>
    public interface ITitleBar : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the width of the system-reserved region of the upper-right corner of the app window.
        /// </summary>
        double SystemOverlayRightInset { get; }

        /// <summary>
        /// Initialize the states of the title bar.
        /// </summary>
        Task SetupTitleBarAsync();
    }
}
