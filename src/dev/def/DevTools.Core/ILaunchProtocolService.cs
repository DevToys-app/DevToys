using System.Threading.Tasks;

namespace DevTools.Core
{
    /// <summary>
    /// Provides a service allowing to start new instance of the app.
    /// </summary>
    public interface ILaunchProtocolService
    {
        /// <summary>
        /// Starts a new instance of the app with an argument.
        /// </summary>
        /// <returns>Returns <code>True</code> if it succeeded.</returns>
        Task<bool> LaunchNewAppInstance(string? arguments = null);
    }
}
