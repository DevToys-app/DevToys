#nullable enable

using System.Threading.Tasks;

namespace DevToys.Api.Core
{
    /// <summary>
    /// Provides a set of methods to interact with the clipboard.
    /// </summary>
    public interface IClipboard
    {
        /// <summary>
        /// Gets the content of the clipboard as a text, if exists.
        /// </summary>
        /// <returns>Returns an empty string if the clipboard doesn't have text.</returns>
        Task<string> GetClipboardContentAsTextAsync();
    }
}
