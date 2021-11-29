#nullable enable

using System.ComponentModel;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Provides information about a tool and create an instance of it.
    /// </summary>
    public interface IToolProvider : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the name of the tool. It will be displayed in the list of tools.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the name of the tool that will be told to the user when using screen reader.
        /// </summary>
        string AccessibleName { get; }

        /// <summary>
        /// Gets an object type that has a width, height and image data. It can be an icon through a font, an SVG...etc.
        /// </summary>
        object IconSource { get; }

        /// <summary>
        /// Creates a new instance of the tool.
        /// </summary>
        IToolViewModel CreateTool();

        /// <summary>
        /// Analyze the data given in parameter and tells whether the current tool can treat it.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is useful for detecting automatically what tool to suggest the user to use based on an input (coming from the clipboard for example).
        /// </remarks>
        bool CanBeTreatedByTool(string data);
    }
}
