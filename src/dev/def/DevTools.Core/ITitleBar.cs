using System.Threading.Tasks;

namespace DevTools.Core
{
    /// <summary>
    /// Provides a service designed to manager the window title bar.
    /// </summary>
    public interface ITitleBar
    {
        /// <summary>
        /// Initialize the states of the title bar.
        /// </summary>
        Task SetupTitleBarAsync();
    }
}
