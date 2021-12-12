#nullable enable

using System.Threading.Tasks;
using DevToys.Api.Tools;

namespace DevToys.Api.Core
{
    /// <summary>
    /// Provides a service allowing to activate the app throught a URI protocol.
    /// </summary>
    public interface IUriActivationProtocolService
    {
        /// <summary>
        /// Starts a new instance of the app with an argument.
        /// </summary>
        /// <returns>Returns <code>True</code> if it succeeded.</returns>
        Task<bool> LaunchNewAppInstance(string? arguments = null);

        /// <summary>
        /// Creates a tile on the Windows start menu that points to a given tool.
        /// </summary>
        /// <param name="toolProvider">The tool to pin</param>
        /// <returns>Returns <code>True</code> if it succeeded.</returns>
        Task<bool> PinToolToStart(MatchedToolProvider toolProvider);
    }
}
